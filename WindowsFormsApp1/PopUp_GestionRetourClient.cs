﻿using System;
using System.IO;
using System.Collections.Generic;
using LettreCooperation.modele;
using LettreCooperation.Model;
using System.Windows.Forms;

namespace LettreCooperation
{
    public partial class RetourClientLC : Form
    {        
        /* Initialisation de ma variable lesClients = contiendra tous les Clients */
        private List<Client> lesClients = null;
        /* Initialisation de ma variable lesLcDuClient = contiendra toutes les LC d'un Client */
        private List<LC> lesLcDuClient = null;
        /* Initialisation lcDuClient = contiendra la LC du Client avec tout ses attributs */
        private LC lcDuClient = null;
        /* Initialisation de ma variable idDeLaLcChoisi - permet d'avoir la LC précisement et éviter de se tromper  */
        int idDeLaLcChoisi = 0;
        /* Initialisation de ma variable lesEtatsLC = contiendra les etats possible des LC */
        private IEnumerable<Etat> lesEtatsLC = null;
        /* Variable contenant la raison sociale du Client choisi */
        private string clientChoisi = "";
        /* Variable qui contiendra tout les id des LC du client
         * BUT : Permettra de les retrouver plus efficacement */
        private List<int> lesIdDesLcDuClient = new List<int>();
        // ToDo | Changer le chemin général "@pathGeneral"
        /* Initialisation du chemin général pour toutes les LC */
       

        ModelManager manager = new ModelManager();

        /* Accesseurs des variables */
        public IEnumerable<Etat> LesEtatsLC { get => lesEtatsLC; set => lesEtatsLC = value; }

        public RetourClientLC()
        {
            InitializeComponent();
            InitialiserLabelEtatLc();

            // Centrage de la page
            this.CenterToScreen();

            // Nous fixons les dimensions de l'application
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            /* Eviter les modifications des valeurs par l'utilisateur dans les combobox 
             * Cela permet de récupérer les bonnes valeurs */
            cbChoixClient.DropDownStyle = ComboBoxStyle.DropDownList;
            /* Eviter les modifications des valeurs par l'utilisateur dans les combobox */
            cbChoixLcDuClient.DropDownStyle = ComboBoxStyle.DropDownList;
            /* On désactive le bouton "Archiver" jusqu'au moment propice 
             * BUT : Eviter l'archivage "null", ce qui poserait problème */
            btnArchiverLC.Enabled = false;
            /* On désactive le bouton "Refuser" jusqu'au moment propice 
            * BUT : Eviter de refuser tout et n'importe quoi, ce qui poserait problème */
            btnRefuserLC.Enabled = false;
            /* Valeur par défaut de l'Etat de la LC */
            //lblEtatLc2.Text = "Choississez un Client et une Lettre de Coopération ";

            /* Variable contenant tous les Clients */
            lesClients = manager.GetListClientRetour();

            if (lesClients == null)
            {
                btnArchiverLC.Enabled = false;
                btnRefuserLC.Enabled = false;
                return;
            }
               
            
            /* Variable contenant tous les Etats d'une LC */
            lesEtatsLC = manager.GetLesEtatsLC();
            
            /* Rempli la premiere liste déroulante
             * Choix du Client  */
            ChargerListeClients(lesClients);
            
        }

        /* Remise à zéro du formulaire complet */
        private void Init()
        {
            cbChoixLcDuClient.Items.Clear();
            cbChoixClient.Items.Clear();
           // lblEtatLc2.ResetText();

            /* Bloque le bouton "Archiver" */
            btnArchiverLC.Enabled = false;
            /* Bloque le bouton "Refuser" */
            btnRefuserLC.Enabled = false;

            ChargerListeClients(lesClients);
            InitialiserLabelEtatLc();
        }


        /* Lorsque l'élément selectionné de la 1ere liste déroulante change
         * On effectue les tâches ci-dessous 
         * Tâches : + Efface tout les items de la 2ème liste déroulante (Elle contient les noms des LC du Client spécifique) 
         *          + Efface le contenu "Text" de la liste déroulante 
         *          + LesLcDuCLient = Variable qui contient toutes les LC du Client Choisi 
         *          + Ajout des LC du Client spécifique dans la 2ème liste déroulante */
        private void CbChoixClient_SelectedIndexChanged(object sender, EventArgs e)
        {
            /* Valeur de l'Etat lorsque l'utilisateur a choisi un Client */
           // lblEtatLc2.Text = "Choississez une Lettre de Coopération ";
           // lblEtatLc2.ForeColor = Color.Red;
            /* Rebloque le bouton "Archiver" */
            btnArchiverLC.Enabled = false;
            /* Rebloque le bouton "Refuser" */
            btnRefuserLC.Enabled = false;

            /* Variable contenant la raison sociale du Client choisi */
            clientChoisi = cbChoixClient.SelectedItem.ToString();
                        
            /* Supprime les composants de la 2ème Liste déroulante 
             * But : Mettre les LC correspondant uniquement au Client choisi */
            cbChoixLcDuClient.Items.Clear();
            cbChoixLcDuClient.ResetText();

            /* LesLcDuCLient = Variable qui contient toutes les LC du Client Choisi  */
            lesLcDuClient = manager.GetListLCFromClienteRetour(
                lesClients[cbChoixClient.SelectedIndex].id_client
                );
            
            /* Ajout des LC du Client spécifique dans la 2ème liste déroulante */
            ChargerListeDesLcDuClient(lesLcDuClient);      
        }

        private void CbChoixLcDuClient_SelectedIndexChanged(object sender, EventArgs e)
        {
            /* Utilisation d'une variable intermédiaire qui me permet de connaitre l'emplacement de la LC dans ma liste 
             * BUT : Retrouver l'id exact de la LC dans la Liste des id des LC du Client 
             *       Les modifications seront précises et cela évitera des erreurs */
            int idEmplacementLcDansLaListeDeroulante = cbChoixLcDuClient.SelectedIndex;
            // MessageBox.Show(idEmplacementLcDansLaListeDeroulante.ToString());

            /* Récupération de l'id de la LC choisie
             * On récupère l'id exacte des LC lors du remplissage du la Liste des LC du Client 
             * BUT : Sauvegarder les id des LC du Client pour les repérer plus rapidement par la suite */
            idDeLaLcChoisi = lesLcDuClient[idEmplacementLcDansLaListeDeroulante].id_lc;
            // MessageBox.Show(idDeLaLcChoisi.ToString());

            /* Récupération de la LC choisie complètement (objet entier) */
            //lcDuClient = manager.getLaLcDuClient(idDeLaLcChoisi, cbChoixLcDuClient.SelectedItem.ToString());
            lcDuClient = lesLcDuClient[idEmplacementLcDansLaListeDeroulante];


            /* Récupère et affiche l'état de la LC choisie */
           // lblEtatLc2.Text = manager.getEtatDeLaLC(lcDuClient);
          //  lblEtatLc2.Text = manager.GetEtatById(lcDuClient.id_etat).libelle_etat;
          //  lblEtatLc2.ForeColor = Color.Black;

            /* Activation du bouton "Archiver" 
             * Toutes les données necessaires sont disponibles */
            btnArchiverLC.Enabled = true;
            /* Activation du bouton "Refuser" 
             * Toutes les données necessaires sont disponibles */
            btnRefuserLC.Enabled = true;
            
        }

        private void BtnArchiverLC_Click(object sender, EventArgs e)
        {

            Cursor.Current = Cursors.WaitCursor;
            /* openFile me permet d'obtenir le fichier que je veux 'déplacer|archiver' 
             * Ici j'initialise ma variable openFile qui va contenir le chemin absolu du fichier */
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Fichiers pdf|*.pdf";
            /* J'ouvre la fenêtre permettant d'aller chercher le fichier à 'déplacer|archiver' */
            openFile.ShowDialog();

            if (openFile.SafeFileName.Length == 0)
            {
                Cursor.Current = Cursors.Default;
                return;
            }


            /* Je sauvegarde le nom du fichier dans une variable à part - nomDeLaLcAArchiver */
            string nomDeLaLcAArchiver = openFile.SafeFileName;
            /* Je sauvegarde le chemin absolu du fichier dans une variable à part - fichierSrc */
            string fichierSrc = openFile.FileName;
            
            /* Essaye d'archiver le fichier */
            try
            {
                /* fichierDest = correspond "chemin du fichier" + "Raison Sociale du Client = dossier du client" + "Nom final du fichier à déplacer" */
             //   string dossierCibleArchive = @"\Interne\5.LC & Prospection\5.Lettres de coopération\zzz_old\";
                string pathDest = Program.FINACOOPFolder + Properties.Settings.Default.PathArchiver + clientChoisi + @"\";
                string fichierDest = "";

                /* Vérifie si le dossier des archives du client existe dans le répertoire  */
                if (!Directory.Exists(pathDest))
                     Directory.CreateDirectory(pathDest);

                fichierDest = pathDest + nomDeLaLcAArchiver;
                
                
                /* File.Move permet de déplacer le fichier d'un dossier à un autre grâce a 2 variables : CHEMIN absolu du fichier SOURCE capté plus haut ET CHEMIN absolu FINAL du fichier archivé déterminé plus haut  */
                File.Move(sourceFileName: fichierSrc, destFileName: fichierDest);
                /* Changement de l'état de la LC qui passe en "Archivée" (libelle de l'Etat d'une LC archivé = "A") dans le programme */
                lcDuClient.id_etat = manager.GetIdEtatArchiver();
                /* La LC a été archivée, son chemin change donc d'adresse */
                lcDuClient.chemin_lc = Properties.Settings.Default.PathArchiver + clientChoisi + @"\" + nomDeLaLcAArchiver;
                /* Sauvegarde du changement de l'état de la LC qui passe en "Archivée" dans la Base De Données */
                manager.SaveBDD();
                /* Message qui indique à l'utilisateur que la LC a bien été archivée */
                MessageBox.Show("Lettre de Coopération Archivée ! \n" + "LC archivée : " + lcDuClient.nom_lc + "\n Client concerné : " + lcDuClient.chemin_lc);

                Init();
            }
            catch (Exception)
            {
                Cursor.Current = Cursors.Default;
                /* Message qui indique à l'utilisateur que la LC n'a pas été archivée */
                 MessageBox.Show("Archivage non réalisé !");
            }
        }

        private void btnRefuserLC_Click(object sender, EventArgs e)
        {

            Cursor.Current = Cursors.WaitCursor;

            /* string dossierCibleRefus = @"LC à réaliser et envoyer";
             string pathDest = pathGeneral + @"\" + dossierCibleRefus + @"\" + clientChoisi;

             // Vérifie l'existence de l'arborescence - elle doit normalement déjà exister
             if (Directory.Exists(pathDest))
             {
                 // Essaye de refuser le fichier
                 try
                 {
                     // Changement de l'état de la LC qui passe en "Refusée" (libelle de l'Etat d'une LC refusée = "R") dans le programme
                     lcDuClient.id_etat = manager.GetIdEtatRefuser();
                     // La LC a été refusée donc elle change de dossier 
                     lcDuClient.chemin_lc = pathDest + @"\" + lcDuClient.nom_lc;

                     // Sauvegarde du changement de l'état de la LC qui passe en "Refusée" dans la Base De Données
                     manager.SaveBDD();
                     // Message qui indique à l'utilisateur que la LC a bien été archivée
                     MessageBox.Show("Lettre de Coopération Refusée ! \n" + "LC Refusée : " + lcDuClient.nom_lc + "\n Client concerné : " + clientChoisi);

                     Init();
                 }
                 catch (Exception)
                 {
                     // Message qui indique à l'utilisateur que la LC n'a pas été archivée
                     MessageBox.Show("Refus non réalisé !");
                 }
             }
             else
             {
                 MessageBox.Show("Chemin inexistant ! \n Vérifier l'existence de la LC");
             }*/


            string pathDest = Properties.Settings.Default.PathRealiser + clientChoisi;
            lcDuClient.chemin_lc = pathDest + @"\" + lcDuClient.nom_lc;
            lcDuClient.id_etat = manager.GetIdEtatRefuser();
            manager.SaveBDD();

            MessageBox.Show("Lettre de Coopération Refusée ! \n" + "LC Refusée : " + lcDuClient.nom_lc + "\n Client concerné : " + clientChoisi);

            Init();

            Cursor.Current = Cursors.Default;
        }

        private void BtnReinitialiserLaFenetre_Click(object sender, EventArgs e)
        {
            this.Init();
           // lblEtatLc2.Text = "Choisissez un Client et une Lettre de Coopération ";
        }

        
        private void ChargerListeClients(IEnumerable<Client> lesClients)
        {
            foreach (var item in lesClients)
            {
                /* Ajoute chaque Client à la liste déroulante */
                cbChoixClient.Items.Add(item.raison_sociale);
            }
        }

        private void ChargerListeDesLcDuClient(List<LC> lesLcDuClient)
        {
            cbChoixLcDuClient.Items.Clear();
            foreach (var item in lesLcDuClient)
            {
                if (File.Exists(Program.FINACOOPFolder + "\\" + item.chemin_lc))
                {
                    cbChoixLcDuClient.Items.Add(item.nom_lc);
                    /* Enregistre tout les id des LC dans une liste
                     * BUT : Récupérer l'id de la LC pour éviter les erreurs de manipulatipon des LC */
                    lesIdDesLcDuClient.Add(item.id_lc);
                }
            }
        }

        private void InitialiserLabelEtatLc()
        {
           // lblEtatLc2.Text = "Choisissez un Client et une Lettre de Coopération";
          //  lblEtatLc2.ForeColor = Color.Red;
        }


    }
}
