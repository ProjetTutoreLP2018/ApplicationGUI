﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using LettreCooperation.modele;
using Microsoft.Office.Interop.Word;
using LettreCooperation.Model;
using Xceed.Words.NET;
using System.Drawing;

namespace LettreCooperation
{
    public partial class Page_SignatureExp : UserControl
    {

        private ModelManager modelManager = new ModelManager();
        private List<LC> listLc = new List<LC>();
       

        public Page_SignatureExp()
        {
            InitializeComponent();
            Init();
            this.textBoxPass.PasswordChar = '•';
        }


        /// <summary>
        /// Initialisation de la page de signature
        /// </summary>
        private void Init()
        {


            LCDataGridView.Rows.Clear();
            LCDataGridView.Refresh();
            listLc = modelManager.GetListLcASigner();

            if (listLc == null)
                return;

            foreach (var item in listLc)
            {
                String[] row = {
                    item.nom_lc ,
                    modelManager.FindClient(item.id_client).raison_sociale,
                    item.date_debut + "",
                    modelManager.FindUtilisateur(item.id_utilisateur).nom_utilisateur + " " + modelManager.FindUtilisateur(item.id_utilisateur).prenom_utilisateur};
                LCDataGridView.Rows.Add(row);

                if (item.id_etat == modelManager.GetIdEtatRefuser())
                    LCDataGridView.Rows[LCDataGridView.Rows.Count - 1].DefaultCellStyle.BackColor = Color.LightSalmon;
                
            }
  
        }


        /// <summary>
        /// Méthode qui permet de gérer la signature
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonSigner_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            string pass = EncryptePass(textBoxPass.Text);

            if (!pass.Equals(Page_Principale.Utilisateur.mdp_utilisateur))
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show("Votre mot de passe est incorrect");
                return;
            }

            if (Page_Principale.Utilisateur.image_Blob_Signature == null)
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show("Vous ne disposez pas de signature dans la base. Veuillez contacter l'Administrateur.");
                return;
            }

            PopUp_Patienter waitForm = new PopUp_Patienter();
            waitForm.Show();

            for (int i = 0; i < LCDataGridView.RowCount; ++i)
            {
                DataGridViewCheckBoxCell chkchecking = LCDataGridView.Rows[i].Cells[4] as DataGridViewCheckBoxCell;


                //MessageBox.Show(chkchecking.Value.ToString());

                if (chkchecking.Value != null && (bool)chkchecking.Value)
                {

                    var app = new Microsoft.Office.Interop.Word.Application();
                    
                     try
                     {
                        //ouverture du document

                        // MessageBox.Show(Program.FINACOOPFolder + listLc[i].chemin_lc);

                        if(listLc[i].id_etat.ToString().Equals(modelManager.GetIdEtatRefuser().ToString()))
                        {
                            if(listLc[i].id_signataire != Page_Principale.Utilisateur.id_utilisateur)
                            {
                                Utilisateur signataireOrigine = modelManager.GetUser(listLc[i].id_signataire);
                                MessageBox.Show("Seul le signataire original de cette LC (" + signataireOrigine.nom_utilisateur + " " + signataireOrigine.prenom_utilisateur + ") ou l'administrateur peut la revalider");
                            } else
                            {
                                modelManager.ChangerEtatLC_Signer(listLc[i].id_lc);
                                AfficherLC(Program.FINACOOPFolder + listLc[i].chemin_lc);
                                MessageBox.Show("Votre fichier a été revalidé");

                            }

                        } else
                        {
                            var doc = app.Documents.Add(
                            Program.FINACOOPFolder + listLc[i].chemin_lc,
                            Visible: false);

                            doc.Activate();

                            //récuperation du mot à remplacer
                            //************************************************
                            var motcle = "signature";
                            // MessageBox.Show("Remplacement du mot: " + motcle + " ...");
                            var sel = app.Selection;
                            sel.Find.Text = string.Format("[" + motcle + "]");
                            sel.Find.Execute(Replace: WdReplace.wdReplaceNone);
                            sel.Range.Select();

                            //Insertion de l'image 
                            // var imgPath = Path.GetFullPath(string.Format(file2));
                            System.Drawing.Image imgPath = ByteArrayToImage(Page_Principale.Utilisateur.image_Blob_Signature);
                            imgPath.Save(Program.FINACOOPFolder + @"\signature.png");

                            sel.InlineShapes.AddPicture(
                                FileName: Program.FINACOOPFolder + @"\signature.png",
                                LinkToFile: false,
                                SaveWithDocument: true);

                            File.Delete(Program.FINACOOPFolder + @"\signature.png");

                            //************************************************

                            //sauvegarde du doc.
                            modelManager.ChangerEtatLC_Signer(listLc[i].id_lc);
                            modelManager.AjoutSignataire(listLc[i], Page_Principale.Utilisateur);
                            doc.SaveAs(Program.FINACOOPFolder + listLc[i].chemin_lc);
                            doc.Close();

                            AjoutNomSignataire(Program.FINACOOPFolder + listLc[i].chemin_lc);
                            AfficherLC(Program.FINACOOPFolder + listLc[i].chemin_lc);
                            MessageBox.Show("Votre fichier a bien été signé");

                        }

                    }
                    catch (Exception ex)
                    {
                        Cursor.Current = Cursors.Default;
                        MessageBox.Show(ex.Message);
                    }
                        
                }

            }

            Init();
            Cursor.Current = Cursors.Default;
            waitForm.Close();
        }


        private string EncryptePass(string pass)
        {

            byte[] data = System.Text.Encoding.ASCII.GetBytes(pass);
            data = new System.Security.Cryptography.SHA256Managed().ComputeHash(data);
            String hash = System.Text.Encoding.ASCII.GetString(data);

            return hash;
        }


        /// <summary>
        /// Méthode qui permet de gérer les fichiers Word
        /// </summary>
        /// <param name="pathOrigine"></param>
        private void AfficherLC(string pathOrigine)
        {

            WordTools.Path = pathOrigine;
            WordTools.OpenWord();

        }


        /// <summary>
        /// Ajout le nom du signataire sur
        /// le document
        /// </summary>
        /// <param name="path"></param>
        private void AjoutNomSignataire(String path)
        {
            DocX documentModele = DocX.Load(path);
            documentModele.ReplaceText("{{PrenomEComptable}}", Page_Principale.Utilisateur.prenom_utilisateur);
            documentModele.ReplaceText("{{NomEComptable}}", Page_Principale.Utilisateur.nom_utilisateur);
            documentModele.SaveAs(path);
        }


        /// <summary>
        /// Gère le mot de passe
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxPass_TextChanged(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(this.textBoxPass.Text))
                this.button1.Enabled = true;
            else
                this.button1.Enabled = false;
        }


        /// <summary>
        /// Méthode qui permet de transformer un tableau
        /// de Byte en image
        /// </summary>
        /// <param name="byteArrayIn"></param>
        /// <returns></returns>
        public System.Drawing.Image ByteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            System.Drawing.Image returnImage = System.Drawing.Image.FromStream(ms);
            return returnImage;
        }


        /// <summary>
        /// Méthode de raffraichissement de la page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBox1_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Init();
            Cursor.Current = Cursors.Default;
        }


        /// <summary>
        /// Méthode qui permet d'ouvrir une LC avant de la signer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LCDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

            if (LCDataGridView.CurrentCell.ColumnIndex == 0)
                AfficherLC(Program.FINACOOPFolder + listLc[LCDataGridView.CurrentCell.RowIndex].chemin_lc);


        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
