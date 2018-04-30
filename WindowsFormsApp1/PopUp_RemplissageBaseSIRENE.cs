﻿using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Windows.Forms;

namespace LettreCooperation
{
    public partial class PopUp_RemplissageBaseSIRENE : Form
    {
        private SIRENE entreprises;
        public Record entrepriseSelectionnee;
        public PopUp_RemplissageBaseSIRENE()
        {
            InitializeComponent();
        }

        private void FenSelectionEntreprise_Load(object sender, EventArgs e)
        {
            
        }

        private async void MotsCles_KeyDownAsync(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage reponse = await client.GetAsync("https://data.opendatasoft.com/api/records/1.0/search/?dataset=sirene%40public&q=" + MotsCles.Text.ToUpper() + "&facet=depet&facet=libcom&facet=siege&facet=libapet&facet=libnj&facet=libapen&facet=ess&facet=libtefen&facet=categorie&facet=nom_dept&facet=section");
                string json = await reponse.Content.ReadAsStringAsync();
                this.entreprises = JsonConvert.DeserializeObject<SIRENE>(json);
                int nombreEntreprises = this.entreprises.nhits;
                if (nombreEntreprises > 10)
                {
                    MessageBox.Show("Votre requête " + MotsCles.Text + " donne " + nombreEntreprises + " résultats.\nVeuillez modifier vos mots-clés ou en rajouter.");
                }
                else if (nombreEntreprises == 0)
                {
                    MessageBox.Show("Votre requête n'a pas retourné de résultats.");
                }
                else
                {
                    foreach(Record enregistrement in this.entreprises.records)
                    {
                        string nom = enregistrement.fields.l1_normalisee;
                        string adresse = enregistrement.fields.l4_normalisee;
                        string codePostal = enregistrement.fields.codpos;
                        string ville = enregistrement.fields.libcom;
                        ListeChoixEntreprises.Items.Add(String.Format("{0} ({1} - {2} {3})", nom, adresse, codePostal, ville));
                    }
                }
            }
        }

        private void ListeChoixEntreprises_DoubleClick(object sender, EventArgs e)
        {
            if(ListeChoixEntreprises.Items.Count != 0)
            {
                this.entrepriseSelectionnee = this.entreprises.records[ListeChoixEntreprises.SelectedIndex];
                DialogResult = DialogResult.OK;
            }
        }

        private async void BoutonRechercher_ClickAsync(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            ListeChoixEntreprises.Items.Clear();
            HttpClient client = new HttpClient();
            string json = "";
            try
            {
                HttpResponseMessage reponse = await client.GetAsync("https://data.opendatasoft.com/api/records/1.0/search/?dataset=sirene%40public&q=" + MotsCles.Text.ToUpper() + "&facet=depet&facet=libcom&facet=siege&facet=libapet&facet=libnj&facet=libapen&facet=ess&facet=libtefen&facet=categorie&facet=nom_dept&facet=section");
                json = await reponse.Content.ReadAsStringAsync();
 
                this.entreprises = JsonConvert.DeserializeObject<SIRENE>(json);
                int nombreEntreprises = this.entreprises.nhits;
                if (nombreEntreprises > 10)
                {
                    MessageBox.Show("Votre requête " + MotsCles.Text + " donne " + nombreEntreprises + " résultats.\nVeuillez modifier vos mots-clés ou en rajouter.");
                }
                else if (nombreEntreprises > 10)
                {
                    MessageBox.Show("Votre requête n'a retourné aucun résultat.");
                }
                else
                {
                    foreach (Record enregistrement in this.entreprises.records)
                    {
                        string nom = enregistrement.fields.l1_normalisee;
                        string adresse = enregistrement.fields.l4_normalisee;
                        string codePostal = enregistrement.fields.codpos;
                        string ville = enregistrement.fields.libcom;
                        ListeChoixEntreprises.Items.Add(String.Format("{0} ({1} - {2} {3})", nom, adresse, codePostal, ville));
                    }
                }

            }
            catch (HttpRequestException)
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show("Vous n'êtes pas connecté à la base de données. Merci" +
                   "de vérifier votre connexion internet ou de vérifier que le port 1433 de votre Box est bien ouvert.");
            }

            Cursor.Current = Cursors.Default;
        }
    }
}
