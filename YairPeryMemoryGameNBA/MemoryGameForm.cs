using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;

namespace YairPeryMemoryGameNBA
{
    public partial class MemoryGameForm : Form
    {
        private Random random = new Random();

        private List<string> icons = new List<string>()
        {
            "!", "!", "N", "N", ",", ",", "k", "k", "b", "b", "v", "v",
            "w", "w", "z", "z", "@", "@", "J", "J", "U", "U", "Y", "Y",
            "x", "x", "r", "r", "n", "n", "a", "a", "~", "~", "%", "%"
        };
        private Label firstClicked, secondClicked;

        public MemoryGameForm()
        {
            InitializeComponent();

            AssignIconsToSquares();
        }

        private void label_Click(object sender, EventArgs e)
        {
            if ((firstClicked != null) && (secondClicked != null))
                return;

            Label clickLabel = sender as Label;

            if (clickLabel == null)
                return;
            if (clickLabel.ForeColor == Color.Black)
                return;
            if (firstClicked == null)
            {
                firstClicked = clickLabel;
                firstClicked.ForeColor = Color.Black;
                return;
            }

            secondClicked = clickLabel;
            secondClicked.ForeColor = Color.Black;

            CheckForWinner();

            if (firstClicked.Text == secondClicked.Text)
            {
                firstClicked = null;
                secondClicked = null;
            }
            else
            {
                timer1.Start();
            }
        }

        private void CheckForWinner()
        {
            Label label;
            for (int i = 0; i < tableLayoutPanel1.Controls.Count; i++)
            {
                label = tableLayoutPanel1.Controls[i] as Label;

                if ((label != null) && (label.ForeColor == label.BackColor))
                    return;
            }

            MessageBox.Show("Congrats! You won the game!");
            Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();

            firstClicked.ForeColor = firstClicked.BackColor;
            secondClicked.ForeColor = secondClicked.BackColor;

            firstClicked = null;
            secondClicked = null;
        }

        private void AssignIconsToSquares()
        { 
            // פעולה זו מקצה סמלים (תמונות) לריבועים 
            // פירוט פעולה: בוחרים מספר אקראי, משתמשים במספר אקראי זה כדי לבחור סמל אקראי ברשימה שלנו
            // אחר כך  מגדירים את הטקסט של התווית שלו לסמל ואז מסירים את הסמל האקראי מרשימת הסמלים שלנו 

            Label label;
            int randomNumber;

            for (int i = 0; i < tableLayoutPanel1.Controls.Count; i++)
            {
                if (tableLayoutPanel1.Controls[i] is Label)
                    label = (Label)tableLayoutPanel1.Controls[i];
                else
                    continue;

                randomNumber = random.Next(0, icons.Count);
                label.Text = icons[randomNumber];

                icons.RemoveAt(randomNumber);
            }
        }
    }
}
