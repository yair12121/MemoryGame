using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YairPeryMemoryGameNBA
{
    public partial class PictureBoxMemoryGameForm : Form
    {
        private const string NBA_TEAMS_PICTURE_FOLDER_PATH = "Pictures\\NBA_Teams";
        private const string QUESTION_MARK_PICTURE_PATH = "Pictures\\question-mark.png";

        private Random random = new Random();

        private List<string> nbaTeamsPictures = new List<string>()
        {
            "Golden_State_Warriors.png",
            "Golden_State_Warriors.png",
            "Boston_Celtics.png",
            "Boston_Celtics.png",
            "Los_Angeles_Lakers.png",
            "Los_Angeles_Lakers.png",
            "brooklyn_nets.png",
            "brooklyn_nets.png",
            "Toronto_Raptors.png",
            "Toronto_Raptors.png",
            "Philadelphia_76ers.png",
            "Philadelphia_76ers.png",
            "Milwaukee_Bucks.png",
            "Milwaukee_Bucks.png",
            "Dallas_Mavericks.png",
            "Dallas_Mavericks.png",
            "Miami_Heat.png",
            "Miami_Heat.png",
            "Chicago_Bulls.png",
            "Chicago_Bulls.png",
            "Minnesota_Timberwolves.png",
            "Minnesota_Timberwolves.png",
            "New_Orleans_Pelicans.png",
            "New_Orleans_Pelicans.png",
            "Denver_Nuggets.png",
            "Denver_Nuggets.png",
            "Atlanta_Hawks.png",
            "Atlanta_Hawks.png",
            "New_York_Knicks.png",
            "New_York_Knicks.png",
            "Cleveland_Cavaliers.png",
            "Cleveland_Cavaliers.png",
            "Charlotte_Hornets.png",
            "Charlotte_Hornets.png",
            "Washington_Wizards.png",
            "Washington_Wizards.png"
        };

        private PictureBox firstClicked, secondClicked;
        private bool canClick = true;
        private int matchPairs = 0;
        private Stopwatch stopWatch = new Stopwatch();

        public PictureBoxMemoryGameForm()
        {
            InitializeComponent();

            this.tmrGameTimer.Start();
            AssignPictures();


            stopWatch.Start();
        }

        private void PictureBoxMemoryGameForm_Load(object sender, EventArgs e)
        {

        }

        private void AssignPictures()
        {
            // פעולה זו מקצה סמלים (תמונות) לריבועים 
            // פירוט פעולה: בוחרים מספר אקראי, משתמשים במספר אקראי זה כדי לבחור סמל אקראי ברשימה שלנו
            // אחר כך  מגדירים את הטקסט של התווית שלו לסמל ואז מסירים את הסמל האקראי מרשימת הסמלים שלנו 

            int randomNumber;

            for (int i = 0; i < tableLayoutPanel1.Controls.Count; i++)
            {
                PictureBox currrentPicture = tableLayoutPanel1.Controls[i] as PictureBox;

                if (currrentPicture == null)
                    continue;

                if (nbaTeamsPictures.Count == 0)
                    break;

                randomNumber = random.Next(0, nbaTeamsPictures.Count);
                string picturePath = nbaTeamsPictures[randomNumber];

                // currrentPicture.Image = Image.FromFile(Path.Combine(PICTURE_FOLDER_PATH, picturePath));
                currrentPicture.Tag = Path.Combine(NBA_TEAMS_PICTURE_FOLDER_PATH, picturePath);
                currrentPicture.Load(QUESTION_MARK_PICTURE_PATH);
                nbaTeamsPictures.RemoveAt(randomNumber);
            }
        }

        private bool IsPictureBoxQuestionMark(PictureBox pictureBox)
        {
            return pictureBox.ImageLocation == "Pictures\\question-mark.png";
        }

        private void pictureBox_Click(object sender, EventArgs e)
        {
            PictureBox clickedPictureBox = sender as PictureBox;

            if (!canClick)
                return;

            if (clickedPictureBox == null)
                return;

            if (!IsPictureBoxQuestionMark(clickedPictureBox))
                return;

            if (firstClicked == null)
            {
                firstClicked = clickedPictureBox;
                firstClicked.Load(firstClicked.Tag.ToString());
                return;
            }

            secondClicked = clickedPictureBox;
            secondClicked.Load(secondClicked.Tag.ToString());
            canClick = false;

            CheckForWinner();

            if (firstClicked.Tag.Equals(secondClicked.Tag))
            {
                firstClicked = null;
                secondClicked = null;
                canClick = true;
                matchPairs++;
                this.lblMatchPairs.Text = matchPairs.ToString();
            }
            else
            {
                tmrNotMatchTimer.Start();
            }
        }

        private void CheckForWinner()
        {
            Label label;
            for (int i = 0; i < tableLayoutPanel1.Controls.Count; i++)
            {
                PictureBox currrentPicture = tableLayoutPanel1.Controls[i] as PictureBox;

                if ((currrentPicture != null) && (IsPictureBoxQuestionMark(currrentPicture)))
                    return;
            }

            MessageBox.Show("Congrats! You won the game!");
            Close();
        }

        private void tmrNotMatchTimer_Tick(object sender, EventArgs e)
        {
            this.tmrNotMatchTimer.Stop();

            firstClicked.Load(QUESTION_MARK_PICTURE_PATH);
            secondClicked.Load(QUESTION_MARK_PICTURE_PATH);

            firstClicked = null;
            secondClicked = null;

            canClick = true;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}",
                        ts.Hours, ts.Minutes, ts.Seconds);
            this.lblGameTimer.Text = elapsedTime;
        }
    }
}
