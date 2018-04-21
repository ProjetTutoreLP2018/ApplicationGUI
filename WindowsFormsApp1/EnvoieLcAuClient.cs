﻿using EASendMail;
using LettreCooperation;
using LettreCooperation.modele;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using WindowsFormsApp1.Model;
using WordToPDF;
using System.IO;

namespace WindowsFormsApp1
{
    public partial class EnvoieLcAuClient : UserControl
    {

        private ModelManager modelManager = new ModelManager();
        private List<LC> listLc;
        private string pathPDF;
        private const string _PATHLCENVOYE = @"\Interne\5.LC & Prospection\5.Lettres de coopération\LC envoyées\";
        private bool envoieMail = false;

        public EnvoieLcAuClient()
        {
            InitializeComponent();
            this.textBoxPass.PasswordChar = '•';
            Init();
        }


        /// <summary>
        /// Méthode d'initialisation du tableau
        /// </summary>
        private void Init()
        {

            dataGridView.Rows.Clear();
            dataGridView.Refresh();

            this.textBoxPass.Text = "";
            buttonEnvoyer.Enabled = false;
            listLc = modelManager.GetListLCWaitingSend();

            for(int i = 0; i < listLc.Count; i++)
            {


                string[] row = {

                    listLc[i].nom_lc,
                    modelManager.FindClient(listLc[i].id_client).raison_sociale,
                    listLc[i].date_debut.ToString(),
                    modelManager.FindUtilisateur(listLc[i].id_utilisateur).nom_utilisateur + " " 
                    + modelManager.FindUtilisateur(listLc[i].id_utilisateur).prenom_utilisateur,
                    modelManager.FindUtilisateur(listLc[i].id_signataire).nom_utilisateur + " "
                    + modelManager.FindUtilisateur(listLc[i].id_signataire).prenom_utilisateur
                };

                dataGridView.Rows.Add(row);
            }

        }


        /// <summary>
        /// Méthode qui permet de vérifier le champ du mot de passe
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxPass_TextChanged(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(this.textBoxPass.Text))
                this.buttonEnvoyer.Enabled = true;
            else
                this.buttonEnvoyer.Enabled = false;
        }


        /// <summary>
        /// Méthode qui permet de valider l'envoir
        /// au clients
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonEnvoyer_Click(object sender, System.EventArgs e)
        {
            for (int i = 0; i < dataGridView.Rows.Count - 1; i++)
            {
                DataGridViewCheckBoxCell chkchecking = dataGridView.Rows[i].Cells[5] as DataGridViewCheckBoxCell;

                //MessageBox.Show(chkchecking.Value.ToString());

                if (chkchecking.Value != null && (bool)chkchecking.Value)
                {

                    CopyLc(listLc[i]);
                    CreatePDF(listLc[i]);
                    SendMailClient(listLc[i], this.textBoxPass.Text);
                    
                    if(envoieMail)
                        ChangeEtat(listLc[i]);

                }
            }

            

            Init();
        }


        /// <summary>
        /// Méthode qui permet l'enoie par mail
        /// </summary>
        /// <param name="lc"></param>
        /// <param name="password"></param>
        private void SendMailClient(LC lc, string password)
        {
            SmtpMail oMail = new SmtpMail("TryIt");
            SmtpClient oSmtp = new SmtpClient();

            Client client = modelManager.FindClient(lc.id_client);

            // Your gmail email address
            string from = PagePrincipale.Utilisateur.email_utilisateur;
            oMail.From = from;

            //Password du type
            string pass = password;

            // Set recipient email address
            string to = client.mail_referent;
            oMail.To = to;

            // Set email subject
            string title = "Voici votre LC";
            oMail.Subject = title;

            // Set email body
            string content = "Bonjour Voici votre LC !";
            oMail.TextBody = content;

            //Test PJ
            string pj = Program.FINACOOPFolder + pathPDF;
            if (!pj.Equals(""))
                oMail.AddAttachment(pj);

            //
            // PROPERTIES /!\
            //
            string smtp;
            smtp = "smtp.gmail.com";
            SmtpServer oServer = new SmtpServer(smtp);

            // Set 587 port, if you want to use 25 port, please change 587 5o 25
            //
            // On peut éventuellement le mettre en properties aussi plutôt qu'en brut afin de s'adapter aux changements sur leur SMTP /!\
            //
            oServer.Port = 587;

            // detect SSL/TLS automatically
            oServer.ConnectType = SmtpConnectType.ConnectSSLAuto;

            // Gmail user authentication
            // For example: your email is "gmailid@gmail.com", then the user should be the same
            oServer.User = from;
            oServer.Password = pass;
            //Console.WriteLine("start to send email over SSL ...");
            try
            {
                oSmtp.SendMail(oServer, oMail);
                MessageBox.Show("Votre mail a bien été envoyé à l'adresse : " + to, "Message envoyé");
                envoieMail = true;
            }
            catch (Exception ep)
            {
                MessageBox.Show("Problème lors de l'envoi du message : Merci de vérifier votre Adresse Mail", "Erreur : Mail", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        /// <summary>
        /// Méthode qui permet le changement d'état de la LC
        /// </summary>
        /// <param name="lc"></param>
        private void ChangeEtat(LC lc)
        {
            modelManager.UpdateEtatLc(lc, 8);
        }


        /// <summary>
        /// Méthode qui permet de copier la LC
        /// dans le nouveau dossier d'attente de réponse du client
        /// </summary>
        /// <param name="lc"></param>
        private void CopyLc(LC lc)
        {

            string dossier = Program.FINACOOPFolder + _PATHLCENVOYE + modelManager.FindClient(lc.id_client).raison_sociale;

            if (!Directory.Exists(dossier))
            {
                Directory.CreateDirectory(dossier);
                File.SetAttributes(dossier, FileAttributes.Normal);

            }

            File.Copy(
                Program.FINACOOPFolder + lc.chemin_lc,
                dossier + "\\" + lc.nom_lc + ".docx",
                true
                );
        }


        /// <summary>
        /// Méthode qui permet de créer le PDF que nous enverons au client
        /// </summary>
        /// <param name="lc"></param>
        private void CreatePDF(LC lc)
        {

            Word2Pdf ObjetWord = new Word2Pdf();
            string dossier = Program.FINACOOPFolder + _PATHLCENVOYE + modelManager.FindClient(lc.id_client).raison_sociale;

            if (!Directory.Exists(dossier))
            {
                Directory.CreateDirectory(dossier);
                File.SetAttributes(dossier, FileAttributes.Normal);

            }


            string nomDuFichierAConvertir = lc.nom_lc + ".docx";
            object CheminDuFichier = Program.FINACOOPFolder + lc.chemin_lc;
            string ExtensionDuFichier = Path.GetExtension(nomDuFichierAConvertir);
            string ExtensionCible = nomDuFichierAConvertir.Replace(ExtensionDuFichier, ".pdf");
            if (ExtensionDuFichier == ".doc" || ExtensionDuFichier == ".docx")
            {
                object DossierCible = dossier + "\\" + ExtensionCible;
                ObjetWord.InputLocation = CheminDuFichier;
                ObjetWord.OutputLocation = DossierCible;
                ObjetWord.Word2PdfCOnversion();
            }

            modelManager.UpdatePathLc(lc, _PATHLCENVOYE + modelManager.FindClient(lc.id_client).raison_sociale + "\\" + lc.nom_lc + ".docx");
            pathPDF = _PATHLCENVOYE + modelManager.FindClient(lc.id_client).raison_sociale + "\\" + ExtensionCible;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Init();
        }
    }
}
