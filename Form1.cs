using NAudio.Wave;
using System.Globalization;
using System.Reflection.Metadata.Ecma335;
using static System.Windows.Forms.DataFormats;

namespace Note_Taking_Application
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            File.WriteAllText(filePath, textBox1.Text);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadNotesFromFile();
            LoadPhotos();
            LoadReminders();
            textBox2.Text = "Find in notes...";
            previousHeight = Height;
            previousWidth = Width;
        }

        private void LoadNotesFromFile()
        {
            if (File.Exists(filePath))
            {
                string notes = File.ReadAllText(filePath);
                textBox1.Text = notes;
                textBox1.Select(0, 0);
            }
        }

        private void LoadPhotos()
        {
            if (File.Exists(photosPath))
            {
                string[] linii = File.ReadAllLines(photosPath);
                listBox1.Items.AddRange(linii);
                if (listBox1.Items.Count > 0)
                {
                    listBox1.SelectedIndex = 0;
                }
            }
        }

        private void LoadReminders()
        {
            if (!string.IsNullOrEmpty(File.ReadAllText(dateAndTimePath)))
            {
                comboBox1.Items.AddRange(File.ReadAllLines(dateAndTimePath));
                comboBox1.Text = comboBox1.Items[0].ToString();
            }
            else
            {
                comboBox1.Text = "dd/MM/yyyy     HH:mm";
            }
        }

        private void SaveNotesToFile()
        {
            string notes = textBox1.Text;
            File.WriteAllText(filePath, notes);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveNotesToFile();
        }

        private int lengthMemory;

        private void SearchTextInTextBox(int step)
        {
            int[] posibleText = SearchPosibleText(textBox2.Text);
            lengthMemory = posibleText.Length - 1;
            if (posibleText.Length > 0)
            {
                textBox1.Select(posibleText[step], textBox2.Text.Length);
                textBox1.Focus();
            }
        }

        private int[] SearchPosibleText(string searchTerm)
        {
            string temp = textBox1.Text;
            int[] textFinded = new int[0];
            int index = temp.IndexOf(searchTerm, StringComparison.CurrentCultureIgnoreCase);
            while (index >= 0)
            {
                Array.Resize(ref textFinded, textFinded.Length + 1);
                textFinded[textFinded.Length - 1] = index + ((textFinded.Length - 1) * searchTerm.Length);
                temp = temp.Remove(index, searchTerm.Length);
                index = temp.IndexOf(searchTerm, StringComparison.CurrentCultureIgnoreCase);
            }

            return textFinded;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            index = -1;
        }

        private void textBox2_GotFocus(object sender, EventArgs e)
        {
            textBox2.Text = "";
        }

        private int index;
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox2.Text == "")
            {
                return;
            }

            try
            {
                index++;
                SearchTextInTextBox(index);
            }
            catch
            {
                SearchTextInTextBox(0);
                index = 0;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox2.Text == "")
            {
                return;
            }

            try
            {
                index--;
                SearchTextInTextBox(index);
            }
            catch
            {
                SearchTextInTextBox(lengthMemory);
                index = lengthMemory;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Imagini|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Adăugarea imaginilor selectate în ListBox
                foreach (string fileName in openFileDialog.FileNames)
                {
                    if (!listBox1.Items.Contains(fileName))
                    {
                        listBox1.Items.Add(fileName);
                        List<string> file = new List<string>();
                        file.Add(fileName);
                        File.AppendAllLines(photosPath, file);
                    }
                }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                // Afisarea imaginii selectate în PictureBox
                string photoPath = listBox1.SelectedItem.ToString();
                if (File.Exists(photoPath))
                {
                    Image image = Image.FromFile(photoPath);
                    pictureBox1.Image = image;
                }

                // Afisarea notitelor selecate in PictureBox
                List<string> photoNotesLine = new List<string>(File.ReadAllLines(photosNotesPath));
                if (photoNotesLine.Count() == listBox1.Items.Count)
                {
                    textBox3.Text = EnterConvertorBack(photoNotesLine[listBox1.SelectedIndex]);
                }
                else
                {
                    photoNotesLine.Add("Photo notes");
                    textBox3.Text = photoNotesLine[listBox1.SelectedIndex];
                }

                File.WriteAllLines(photosNotesPath, photoNotesLine);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // Eliminarea imaginei selectate din ListBox și PictureBox, eliminarea notiei de la poza.
            if (listBox1.SelectedIndex != -1)
            {
                List<string> lines = new List<string>(File.ReadAllLines(photosPath));
                lines.RemoveAt(listBox1.SelectedIndex);
                File.Delete(photosPath);
                File.WriteAllLines(photosPath, lines);

                List<string> photoNotesLine = new List<string>(File.ReadAllLines(photosNotesPath));
                photoNotesLine.RemoveAt(listBox1.SelectedIndex);
                File.Delete(photosNotesPath);
                File.WriteAllLines(photosNotesPath, photoNotesLine);

                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
                pictureBox1.Image = null;
                textBox3.Text = null;
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (listBox1.Items.Count > 0 && listBox1.SelectedIndex >= 0)
            {
                List<string> photoNotesLine = new List<string>(File.ReadAllLines(photosNotesPath));
                if (photoNotesLine.Count() == listBox1.Items.Count)
                {
                    photoNotesLine[listBox1.SelectedIndex] = EnterConvertor(textBox3.Text);
                }
                else if (photoNotesLine.Count() <= listBox1.SelectedIndex)
                {
                    photoNotesLine.Add("Photo notes");
                    textBox3.Text = photoNotesLine[listBox1.SelectedIndex];
                }

                File.WriteAllLines(photosNotesPath, photoNotesLine);
            }
        }

        private string EnterConvertor(string text)
        {
            if (text.Contains("\r\n"))
            {
                text = text.Replace("\r\n", "\\r\\n");
            }

            return text;
        }

        private string EnterConvertorBack(string text)
        {

            if (text.Contains("\\r\\n"))
            {
                text = text.Replace("\\r\\n", "\r\n");
            }

            return text;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (!File.Exists(dateAndTimePath))
            {
                File.Create(dateAndTimePath);
            }

            List<string> list = new List<string>(File.ReadAllLines(dateAndTimePath));
            string selectedDateTimeText = dateTimePicker1.Value.ToString("dd/MM/yyyy     HH:mm");
            if (!comboBox1.Items.Contains(selectedDateTimeText) && dateTimePicker1.Value.TimeOfDay > DateTime.Now.TimeOfDay)
            {
                comboBox1.Items.Add(selectedDateTimeText);
                comboBox1.Text = selectedDateTimeText;
                list.Add(selectedDateTimeText);
                File.WriteAllLines(dateAndTimePath, list);
                reminders.Add(dateTimePicker1.Value);
                notifyIcon1.ShowBalloonTip(5000, "Reminder", $"You have set a reminder on {dateTimePicker1.Value.ToString("dd/MM/yyyy")} at {dateTimePicker1.Value.ToString("HH:mm")}.", ToolTipIcon.Info);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (comboBox1.Items.Count > 0)
            {
                List<string> list = new List<string>(File.ReadAllLines(dateAndTimePath));
                list.Remove(comboBox1.Text);
                File.WriteAllLines(dateAndTimePath, list);
                DeleteFromReminders(comboBox1.Text);
                comboBox1.Items.Remove(comboBox1.Text);
                if (comboBox1.Items.Count > 0)
                {
                    comboBox1.Text = comboBox1.Items[0].ToString();
                }
                else
                {
                    comboBox1.Text = "dd/MM/yyyy     HH:mm";
                }
            }
        }

        private List<DateTime> reminders = new List<DateTime>();

        private void timer1_Tick(object sender, EventArgs e)
        {
            label7.Text = DateTime.Now.ToString("dd/MM/yyyy     HH:mm:ss"); // date and time
            foreach (DateTime reminder in reminders)
            {
                if (DateTime.Now.Hour == reminder.Hour &&
                    DateTime.Now.Minute == reminder.Minute &&
                    DateTime.Now.Second == reminder.Second)
                {
                    notifyIcon1.ShowBalloonTip(5000, "Reminder", $"This is a reminder set on {reminder.ToString("dd/MM/yyyy")} at {reminder.ToString("HH:mm")}.", ToolTipIcon.Info);
                }
            }
        }

        private void DeleteFromReminders(string dateTime)
        {
            string format = "dd/MM/yyyy     HH:mm";

            foreach (DateTime reminder in reminders.ToList())
            {
                if (DateTime.TryParseExact(dateTime, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime temp))
                {
                    if (temp.Hour == reminder.Hour &&
                        temp.Minute == reminder.Minute &&
                        temp.Day == reminder.Day &&
                        temp.Month == reminder.Month)
                    {
                        reminders.Remove(reminder);
                        return;
                    }
                }
            }
        }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            waveWriter.Write(e.Buffer, 0, e.BytesRecorded);
        }

        bool isRecording = true;
        private void button8_Click(object sender, EventArgs e)
        {
            if (!isRecording)
            {
                notifyIcon1.ShowBalloonTip(1000, "Reminder", "The voice recording is not currently in progress. Please start the voice recording first.", ToolTipIcon.Info);
                return;
            }

            waveIn.StopRecording();
            waveIn.Dispose();
            waveWriter.Dispose();
            notifyIcon1.ShowBalloonTip(1000, "Reminder", "The voice recording has finished. The file name is \"recording.wav\".", ToolTipIcon.Info);
            isRecording = false;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists(recordingFilePath))
                {
                    File.Delete(recordingFilePath);
                }

                waveIn = new WaveInEvent();
                waveIn.WaveFormat = new WaveFormat(44100, WaveIn.GetCapabilities(waveIn.DeviceNumber).Channels);
                waveIn.DataAvailable += WaveIn_DataAvailable;
                waveWriter = new WaveFileWriter(recordingFilePath, waveIn.WaveFormat);
                notifyIcon1.ShowBalloonTip(1000, "Reminder", "The voice recording has started.", ToolTipIcon.Info);
                isRecording = true;
                waveIn.StartRecording();
            }

            catch
            {

                notifyIcon1.ShowBalloonTip(1000, "Reminder", "Voice recording is currently in progress. Please stop the voice recording first.", ToolTipIcon.Info);
            }
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (Height != previousHeight)
            {
                textBox1.Height += Height - previousHeight;

                pictureBox1.Height += Height - previousHeight;

                textBox3.Location = NewLocationY(textBox3.Location);

                label7.Location = NewLocationY(label7.Location);
            }

            previousHeight = Height;

            if (Width != previousWidth)
            {
                label3.Width += Width - previousWidth;
                listBox1.Width += Width - previousWidth;
                pictureBox1.Width += Width - previousWidth;
                textBox3.Width += Width - previousWidth;

                button5.Location = NewLocationX(button5.Location);
                button6.Location = NewLocationX(button6.Location);
                label4.Location = NewLocationX(label4.Location);
                button7.Location = NewLocationX(button7.Location);
                button8.Location = NewLocationX(button8.Location);
                label5.Location = NewLocationX(label5.Location);
                dateTimePicker1.Location = NewLocationX(dateTimePicker1.Location);
                label6.Location = NewLocationX(label6.Location);
                comboBox1.Location = NewLocationX(comboBox1.Location);
                label8.Location = NewLocationX(label8.Location);
            }

            previousWidth = Width;
        }

        private Point NewLocationY( Point point)
        {
            point.Y += Height - previousHeight;
            return point;
        }
        private Point NewLocationX(Point point)
        {
            point.X += label3.Location.X + label3.Width + 7;
            return point;
        }
    }
}