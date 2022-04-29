using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
        private const string PICTURE_FOLDER_PATH = "Pictures\\NBA_Teams";

        private Random random = new Random();
        private List<string> nbaTeamsPictures = new List<string>()
        {
            "Golden_State_Warriors.png",
            "Golden_State_Warriors.png",
            "Boston_Celtics.png",
            "Boston_Celtics.png"
        };

        public PictureBoxMemoryGameForm()
        {
            InitializeComponent();

            this.tmrGameTimer.Start();
            AssignPictures();
        }

        private void PictureBoxMemoryGameForm_Load(object sender, EventArgs e)
        {

        }

        private void AssignPictures()
        {
            // פעולה זו מקצה סמלים (תמונות) לריבועים 
            // פירוט פעולה: בוחרים מספר אקראי, משתמשים במספר אקראי זה כדי לבחור סמל אקראי ברשימה שלנו
            // אחר כך  מגדירים את הטקסט של התווית שלו לסמל ואז מסירים את הסמל האקראי מרשימת הסמלים שלנו 

            Label label;
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
                currrentPicture.Load(Path.Combine(PICTURE_FOLDER_PATH, picturePath));
                nbaTeamsPictures.RemoveAt(randomNumber);
            }
        }

        private void pictureBox_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {

        }
    }
}
