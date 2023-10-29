﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Cyberpunk_RED_GM_Assistant
{
    public partial class Form1 : Form
    {
        public const string characterConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\characterDb.mdf;Integrated Security=True";
        public const string weaponConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\WeaponDB.mdf;Integrated Security=True";
        private CharacterDatabase characterDatabase;
        private WeaponDatabase weaponDatabase;
        public List<Character> characters;
        public List<Character> charsInQueue; // List of characters in Initiative Queue
        public Character activeCharacter;
        public Character focusedCharacter;
        public Character selectedCharacter;
        public Character targetedCharacter;

        // list of different action panels, e.g. attack panel, reload panel
        // need this so that all panels can be looped over and all but one can be hidden
        // for each panel in panels if (panel != activePanel) hide panel
        public List<Panel> actionPanels;

        public Form1()
        {
            InitializeComponent();

            characterDatabase = new CharacterDatabase(characterConnectionString);
            weaponDatabase = new WeaponDatabase(weaponConnectionString);

            // Load all characters and store into a list
            characters = characterDatabase.GetAllCharacters();
            charsInQueue = new List<Character>(); // placeholder for list of character IDs

            AddToQueue(characters[0]);
            AddToQueue(characters[1]);
            AddToQueue(characters[2]);
            AddToQueue(characters[3]);
            AddToQueue(characters[4]);
            AddToQueue(characters[5]);
            activeCharacter = charsInQueue[0];

            actionPanels = new List<Panel>
            {
                attackPnl,
                attackRollPnl
            };

            UpdateCurrentTurn();
        }

        private void ShowPanel(Panel p)
        {
            // Do not allow the active character to have multiple turns in a row
            if(activeCharacter.turnUsed)
            {
                return;
            }

            foreach(Panel panel in actionPanels) 
            {
                if(panel != p)
                {
                    panel.Hide();
                }
                else
                {
                    panel.Show();
                }
            }
        }

        private void HideActionPanels()
        {
            foreach(Panel panel in actionPanels)
            {
                panel.Hide();
            }
        }

        // Adds a character to the initiative queue
        public void AddToQueue(Character c)
        {
            foreach(Character character in charsInQueue)
            {
                if(character == c)
                {
                    return;
                }
            }
            charsInQueue.Add(c);
            UpdateInitiativeQueue();
        }

        // Overload with int instead of character
        public void AddToQueue(int characterID)
        {
            Character c = characterDatabase.GetCharacterByID(characterID);

            foreach (Character character in charsInQueue)
            {
                if (character == c)
                {
                    return;
                }
            }
            charsInQueue.Add(c);
            UpdateInitiativeQueue();
        }

        // Clears the initiative queue panel and refills it with updated list
        //private void UpdateInitiativeQueue()
        public void UpdateInitiativeQueue()
        {
            queueFPnl.Controls.Clear();

            for(int i = 0; i < charsInQueue.Count; i++)
            {
                FlowLayoutPanel characterPanel = new FlowLayoutPanel();
                characterPanel.Size = new Size(255, 60);
                characterPanel.FlowDirection = FlowDirection.TopDown;
                characterPanel.ContextMenuStrip = contextMenuStrip1;

                Label nameLabel = new Label();
                nameLabel.AutoSize = true;
                nameLabel.Font = new Font(nameLabel.Font.Name, 15f);
                //nameLabel.Text = $"{i + 1}. {charsInQueue[i].Name}";
                nameLabel.Text = charsInQueue[i].CurrentHp <= 0 ? $"X. {charsInQueue[i].Name}" : $"{i + 1}. {charsInQueue[i].Name}";
                characterPanel.Controls.Add(nameLabel);

                Label statsLabel = new Label();
                statsLabel.AutoSize = true;
                statsLabel.Font = new Font(statsLabel.Font.Name, 13f);
                string statsText = $"HP {charsInQueue[i].CurrentHp} | SP {charsInQueue[i].Helmet} | SP {charsInQueue[i].BodyArmor}";
                statsLabel.Text = statsText;
                characterPanel.Controls.Add(statsLabel);

                nameLabel.MouseDown += new MouseEventHandler(QueueLabelClick);
                characterPanel.MouseDown += new MouseEventHandler(QueuePanelClick);

                queueFPnl.Controls.Add(characterPanel);
            }
        }

        private void QueueLabelClick(object sender, MouseEventArgs e)
        {
            Label label = (Label)sender;
            Character character;
            if (label != null)
            {
                string str = label.Text;
                string[] split = str.Split(new string[] { ". " }, StringSplitOptions.None);
                character = GetCharacterByName(split[1]);
                selectedCharacter = character;
            }
        }

        private void QueuePanelClick(object sender, MouseEventArgs e)
        {
            Panel panel = (Panel)sender;
            Character character;
            if (panel.Controls[1] != null)
            {
                string str = panel.Controls[0].Text;
                string[] split = str.Split(new string[] { ". " }, StringSplitOptions.None);
                character = GetCharacterByName(split[1]);
                selectedCharacter = character;
            }
        }

        private Character GetCharacterByName(string name)
        {
            foreach (Character character in characters)
            {
                if (character.Name == name)
                {
                    return character;
                }
            }
            return null;
        }

        // Updates the current turn panel with the active character's attributes
        private void UpdateCurrentTurn()
        {
            currentNameLbl.Text = activeCharacter.Name;
            currentHpLbl.Text = activeCharacter.CurrentHp.ToString();
            maxHpLbl.Text = activeCharacter.MaxHp.ToString();
            currentHelmetLbl.Text = activeCharacter.Helmet.ToString();
            maxHelmetLbl.Text = activeCharacter.Helmet.ToString();
            currentBodyArmorLbl.Text = activeCharacter.BodyArmor.ToString();
            maxBodyArmorLbl.Text = activeCharacter.BodyArmor.ToString();
            label3.Text = $"Select an Action for {activeCharacter.Name}";

            AddWeapons(activeCharacter, weaponsFPnl);

            // Ensures active character and focused character panels never display the same character's information
            if(focusedCharacter != null)
            {
                UpdateFocusChar();
            }

            HideActionPanels();
        }

        // Updates the focused character panel with the focused character's attributes
        private void UpdateFocusChar()
        {
            if(activeCharacter == focusedCharacter)
            {
                focusNameLbl.Text = "";
                focusCurrentHpLbl.Text = "-";
                focusMaxHpLbl.Text = "-";
                focusCurrentHelmetLbl.Text = "-";
                focusMaxHelmetLbl.Text = "-";
                focusCurrentBodyArmorLbl.Text = "-";
                focusMaxBodyArmorLbl.Text = "-";
                focusWeaponsFPnl.Controls.Clear();
                focusedCharacter = null;
                return;
            }

            focusNameLbl.Text = focusedCharacter.Name;
            focusCurrentHpLbl.Text = focusedCharacter.CurrentHp.ToString();
            focusMaxHpLbl.Text = focusedCharacter.MaxHp.ToString();
            focusCurrentHelmetLbl.Text = focusedCharacter.Helmet.ToString();
            focusMaxHelmetLbl.Text = focusedCharacter.Helmet.ToString();
            focusCurrentBodyArmorLbl.Text = focusedCharacter.BodyArmor.ToString();
            focusMaxBodyArmorLbl.Text = focusedCharacter.BodyArmor.ToString();

            AddWeapons(focusedCharacter, focusWeaponsFPnl);
        }

        private void UpdateCombatScreen()
        {
            UpdateInitiativeQueue();
            if(activeCharacter != null)
            {
                UpdateCurrentTurn();
            }
            if(focusedCharacter != null)
            {
                UpdateFocusChar();
            }
        }

        // needs character id in parameters to generate label with correct conditions
        private void AddConditions()
        {
            // for each condition create a new panel with text and add to the conditions flow panel
            Panel conditionPanel = new Panel();
            conditionPanel.Size = new Size(110, 25);

            Label conditionLabel = new Label();
            conditionLabel.AutoSize = true;
            conditionLabel.Font = new Font(conditionLabel.Font.Name, 13f);
            conditionLabel.Text = $"Condition"; // find condition here
            conditionPanel.Controls.Add(conditionLabel);

            conditionsFPnl.Controls.Add(conditionPanel);
        }

        // needs character id in parameters to get weapon IDs
        // then searches for weapon IDs and formats attributes into panels
        private void AddWeapons(Character c, FlowLayoutPanel panel)
        {
            panel.Controls.Clear();

            foreach(Weapon w in c.weaponList)
            {
                // for each weapon create a new flow layout panel and add to the weapons flow panel
                FlowLayoutPanel weaponPanel = new FlowLayoutPanel();
                weaponPanel.Size = new Size(372, 75);
                weaponPanel.FlowDirection = FlowDirection.LeftToRight;

                // Weapon name
                Label nameLabel = new Label();
                nameLabel.Size = new Size(180, 20);
                nameLabel.Font = new Font(nameLabel.Font.Name, 13f);
                nameLabel.Text = $"{w.name}"; // find name here
                weaponPanel.Controls.Add(nameLabel);

                // Weapon type
                Label typeLabel = new Label();
                typeLabel.Size = new Size(180, 20);
                typeLabel.Font = new Font(typeLabel.Font.Name, 13f);
                if(w.isRangedWeapon())
                {
                    RangedWeapon r = (RangedWeapon)w;
                    typeLabel.Text = $"{r.type}";
                }
                else
                {
                    MeleeWeapon m = (MeleeWeapon)w;
                    typeLabel.Text = $"{m.type} Melee";
                }

                weaponPanel.Controls.Add(typeLabel);

                // Weapon damage
                Label dmgLabel = new Label();
                dmgLabel.Size = new Size(180, 20);
                dmgLabel.Font = new Font(dmgLabel.Font.Name, 13f);
                dmgLabel.Text = $"DMG {w.damageDiceAmount}d{w.damageDiceType}";
                weaponPanel.Controls.Add(dmgLabel);

                // Weapon rate of fire
                Label rofLabel = new Label();
                rofLabel.Size = new Size(180, 20);
                rofLabel.Font = new Font(rofLabel.Font.Name, 13f);
                rofLabel.Text = $"ROF {w.ROF}";
                weaponPanel.Controls.Add(rofLabel);

                // Only run this if ranged weapon
                if(w.isRangedWeapon())
                {
                    RangedWeapon r = (RangedWeapon)w;

                    // Current Ammo / Reserve Ammo
                    Label ammoLabel = new Label();
                    ammoLabel.Size = new Size(180, 20);
                    ammoLabel.Font = new Font(ammoLabel.Font.Name, 13f);
                    ammoLabel.Text = $"{r.magazineAmmoCount} / {r.reserveAmmoCount}";
                    weaponPanel.Controls.Add(ammoLabel);

                    // Ammo type (WIP, currently has no functional use)
                    Label ammoTypeLabel = new Label();
                    ammoTypeLabel.Size = new Size(180, 20);
                    ammoTypeLabel.Font = new Font(ammoTypeLabel.Font.Name, 13f);
                    ammoTypeLabel.Text = $"{r.type} Ammo";
                    weaponPanel.Controls.Add(ammoTypeLabel);
                }
                
                panel.Controls.Add(weaponPanel);
            }
        }

        // Prints a string to the combat log
        private void PrintCombatLog(string str)
        {
            Panel logPanel = new Panel();
            logPanel.AutoSize = true;

            Label logLabel = new Label();
            logLabel.MaximumSize = new Size(600, 0);
            logLabel.AutoSize = true;
            logLabel.Font = new Font(logLabel.Font.Name, 13f);
            logLabel.ForeColor = Color.White;
            logLabel.Text = str;
            logPanel.Controls.Add(logLabel);

            combatLogFPnl.Controls.Add(logPanel);
        }

        // Sets up the attack panel
        private void InitialiseAttackPanel()
        {
            // Weapon select
            // for each weapon in character's weapons
            weaponCBox.Items.Add(weaponDatabase.GetWeaponByID(activeCharacter.Weapons).name);

            // Add all characters in the initiative queue to selectable targets
            // excluding the active character
            foreach(Character c in charsInQueue)
            {
                if(activeCharacter != c)
                {
                    targetCBox.Items.Add(c.Name);
                }
            }
        }

        private void ProcessAttackAction()
        {
            // Assign the targeted character
            foreach(Character c in charsInQueue)
            {
                if(c == GetCharacterByName(targetCBox.SelectedItem.ToString()))
                {
                    targetedCharacter = c;
                }
            }

            // Assign the weapon being used
            Weapon useWeapon = activeCharacter.weaponList[0];

            foreach(Weapon w in activeCharacter.weaponList)
            {
                if(w.name == weaponCBox.SelectedItem.ToString())
                {
                    useWeapon = w;
                    break;
                }
            }

            // Determine the difficulty value
            int dv = 99;
            RangedWeapon r = new RangedWeapon();
            MeleeWeapon m = new MeleeWeapon();
            if(useWeapon.isRangedWeapon())
            {
                r = (RangedWeapon)useWeapon;
                r.ShotsFired(); // Subtracts ammo from magazine
                dv = RangedDV((int)r.type, Convert.ToInt32(distanceTBox.Text));
            }
            else
            {
                m = (MeleeWeapon)useWeapon;
                dv = focusedCharacter.Evasion + focusedCharacter.Dexterity + RollDice(1, 10)[0];
            }

            int roll = Convert.ToInt32(attackRollTBox.Text);
            
            // Aimed shots rulebook page 171
            // If not hipfiring then subtract 8 from roll
            if(aimCBox.SelectedItem != aimCBox.Items[0])
            {
                roll -= 8;
            }

            // Check if attack hits
            if(roll > dv)
            {
                // attack hits
                // show roll damage panel
                PrintCombatLog("Attack hit!");
                ShowPanel(attackRollPnl);
            }
            else
            {
                // Attack misses
                activeCharacter.turnUsed = true;
                PrintCombatLog($"{activeCharacter.Name} tried to attack {targetedCharacter.Name} but missed.");
                HideActionPanels();
            }

            UpdateCombatScreen();
        }

        private void ProcessDamageRoll()
        {
            List<TextBox> tBoxes = new List<TextBox>()
            {
                rollDmgTBox1, rollDmgTBox2, rollDmgTBox3, rollDmgTBox4, rollDmgTBox5, 
                rollDmgTBox6, rollDmgTBox7, rollDmgTBox8, rollDmgTBox9
            };
            int roll = 0;

            foreach(TextBox tBox in tBoxes)
            {
                if (int.TryParse(tBox.Text, out int i))
                {
                    roll += i;
                }
            }

            // subtract damage here

            PrintCombatLog($"{roll} damage dealt!");
        }

        // Returns a list of random integers
        private List<int> RollDice(int diceAmount, int diceType)
        {
            Random rnd = new Random();
            List<int> rolls = new List<int>();

            for(int i = 0; i < diceAmount; i++)
            {
                rolls.Add(rnd.Next(1, diceType + 1));
            }

            return rolls;
        }

        // Returns an integer for a ranged attack difficulty value given a weapon type and distance
        private int RangedDV(int type, int distance)
        {
            int dv = 99;
            switch(type)
            {
                case 0:
                    switch(distance)
                    {
                        case int i when (i >= 0 && i <= 6):
                            dv = 13; 
                            break;
                        case int i when (i >= 7 && i <= 12):
                            dv = 15;
                            break;
                        case int i when (i >= 13 && i <= 25):
                            dv = 20;
                            break;
                        case int i when (i >= 26 && i <= 50):
                            dv = 25;
                            break;
                        case int i when (i >= 51 && i <= 100):
                            dv = 30;
                            break;
                        case int i when (i >= 101 && i <= 200):
                            dv = 30;
                            break;
                        case int i when (i >= 201 && i <= 400):
                            dv = 99;
                            break;
                        case int i when (i >= 401 && i <= 800):
                            dv = 99;
                            break;
                        default:
                            dv = 99;
                            break;
                    }
                    break;
                case 1:
                    switch (distance)
                    {
                        case int i when (i >= 0 && i <= 6):
                            dv = 15;
                            break;
                        case int i when (i >= 7 && i <= 12):
                            dv = 13;
                            break;
                        case int i when (i >= 13 && i <= 25):
                            dv = 15;
                            break;
                        case int i when (i >= 26 && i <= 50):
                            dv = 20;
                            break;
                        case int i when (i >= 51 && i <= 100):
                            dv = 25;
                            break;
                        case int i when (i >= 101 && i <= 200):
                            dv = 25;
                            break;
                        case int i when (i >= 201 && i <= 400):
                            dv = 30;
                            break;
                        case int i when (i >= 401 && i <= 800):
                            dv = 99;
                            break;
                        default:
                            dv = 99;
                            break;
                    }
                    break;
                case 2:
                    switch (distance)
                    {
                        case int i when (i >= 0 && i <= 6):
                            dv = 13;
                            break;
                        case int i when (i >= 7 && i <= 12):
                            dv = 15;
                            break;
                        case int i when (i >= 13 && i <= 25):
                            dv = 20;
                            break;
                        case int i when (i >= 26 && i <= 50):
                            dv = 25;
                            break;
                        case int i when (i >= 51 && i <= 100):
                            dv = 30;
                            break;
                        case int i when (i >= 101 && i <= 200):
                            dv = 35;
                            break;
                        case int i when (i >= 201 && i <= 400):
                            dv = 99;
                            break;
                        case int i when (i >= 401 && i <= 800):
                            dv = 99;
                            break;
                        default:
                            dv = 99;
                            break;
                    }
                    break;
                case 3:
                    switch (distance)
                    {
                        case int i when (i >= 0 && i <= 6):
                            dv = 17;
                            break;
                        case int i when (i >= 7 && i <= 12):
                            dv = 16;
                            break;
                        case int i when (i >= 13 && i <= 25):
                            dv = 15;
                            break;
                        case int i when (i >= 26 && i <= 50):
                            dv = 13;
                            break;
                        case int i when (i >= 51 && i <= 100):
                            dv = 15;
                            break;
                        case int i when (i >= 101 && i <= 200):
                            dv = 20;
                            break;
                        case int i when (i >= 201 && i <= 400):
                            dv = 25;
                            break;
                        case int i when (i >= 401 && i <= 800):
                            dv = 30;
                            break;
                        default:
                            dv = 99;
                            break;
                    }
                    break;
                case 4:
                    switch (distance)
                    {
                        case int i when (i >= 0 && i <= 6):
                            dv = 30;
                            break;
                        case int i when (i >= 7 && i <= 12):
                            dv = 25;
                            break;
                        case int i when (i >= 13 && i <= 25):
                            dv = 25;
                            break;
                        case int i when (i >= 26 && i <= 50):
                            dv = 20;
                            break;
                        case int i when (i >= 51 && i <= 100):
                            dv = 15;
                            break;
                        case int i when (i >= 101 && i <= 200):
                            dv = 16;
                            break;
                        case int i when (i >= 201 && i <= 400):
                            dv = 17;
                            break;
                        case int i when (i >= 401 && i <= 800):
                            dv = 20;
                            break;
                        default:
                            dv = 99;
                            break;
                    }
                    break;
                case 5:
                    switch (distance)
                    {
                        case int i when (i >= 0 && i <= 6):
                            dv = 15;
                            break;
                        case int i when (i >= 7 && i <= 12):
                            dv = 13;
                            break;
                        case int i when (i >= 13 && i <= 25):
                            dv = 15;
                            break;
                        case int i when (i >= 26 && i <= 50):
                            dv = 17;
                            break;
                        case int i when (i >= 51 && i <= 100):
                            dv = 20;
                            break;
                        case int i when (i >= 101 && i <= 200):
                            dv = 22;
                            break;
                        case int i when (i >= 201 && i <= 400):
                            dv = 99;
                            break;
                        case int i when (i >= 401 && i <= 800):
                            dv = 99;
                            break;
                        default:
                            dv = 99;
                            break;
                    }
                    break;
                case 6:
                    switch (distance)
                    {
                        case int i when (i >= 0 && i <= 6):
                            dv = 16;
                            break;
                        case int i when (i >= 7 && i <= 12):
                            dv = 15;
                            break;
                        case int i when (i >= 13 && i <= 25):
                            dv = 15;
                            break;
                        case int i when (i >= 26 && i <= 50):
                            dv = 17;
                            break;
                        case int i when (i >= 51 && i <= 100):
                            dv = 20;
                            break;
                        case int i when (i >= 101 && i <= 200):
                            dv = 22;
                            break;
                        case int i when (i >= 201 && i <= 400):
                            dv = 25;
                            break;
                        case int i when (i >= 401 && i <= 800):
                            dv = 99;
                            break;
                        default:
                            dv = 99;
                            break;
                    }
                    break;
                case 7:
                    switch (distance)
                    {
                        case int i when (i >= 0 && i <= 6):
                            dv = 17;
                            break;
                        case int i when (i >= 7 && i <= 12):
                            dv = 16;
                            break;
                        case int i when (i >= 13 && i <= 25):
                            dv = 15;
                            break;
                        case int i when (i >= 26 && i <= 50):
                            dv = 15;
                            break;
                        case int i when (i >= 51 && i <= 100):
                            dv = 20;
                            break;
                        case int i when (i >= 101 && i <= 200):
                            dv = 20;
                            break;
                        case int i when (i >= 201 && i <= 400):
                            dv = 25;
                            break;
                        case int i when (i >= 401 && i <= 800):
                            dv = 30;
                            break;
                        default:
                            dv = 99;
                            break;
                    }
                    break;
            }
            return dv;
        }

        private void queueLabel_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {

        }

        private void queueFlowPanel_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, queueFPnl.ClientRectangle, 
                Color.Red, 2, ButtonBorderStyle.Solid,
                Color.Red, 2, ButtonBorderStyle.Solid,
                Color.Red, 2, ButtonBorderStyle.Solid,
                Color.Red, 2, ButtonBorderStyle.Solid);
        }

        private void conditionsFlowPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void attackRollBtn_Click(object sender, EventArgs e)
        {
            attackRollTBox.Text = Convert.ToString(RollDice(1, 10)[0]);
        }

        private void button1_Click(object sender, EventArgs e)
        {
           
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void viewAllCharactersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //ViewAllCharacters viewAllForm = new ViewAllCharacters();
            ViewAllCharacters viewAllForm = new ViewAllCharacters(this);
            viewAllForm.Show();
        }

        private void executeAttackBtn_Click(object sender, EventArgs e)
        {
            ProcessAttackAction();
        }

        private void executeDmgRollBtn_Click(object sender, EventArgs e)
        {
            ProcessDamageRoll();
        }

        private void rollDmgBtn_Click(object sender, EventArgs e)
        {

        }

        private void viewAllCharactersToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //ViewAllCharacters viewAllForm = new ViewAllCharacters();
            ViewAllCharacters viewAllForm = new ViewAllCharacters(this);
            viewAllForm.Show();
        }

        private void attackBtn_Click(object sender, EventArgs e)
        {
            ShowPanel(attackPnl);
            InitialiseAttackPanel();
        }

        private void viewDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(selectedCharacter == null)
            {
                return;
            }

            Form3 viewDetails = new Form3(selectedCharacter.ID);
            viewDetails.Show();
        }

        private void removeFromQueueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(selectedCharacter == null)
            {
                return;
            }

            charsInQueue.Remove(selectedCharacter);
            UpdateInitiativeQueue();
        }

        private void startTurnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(selectedCharacter == null)
            {
                return;
            }

            activeCharacter = selectedCharacter;
            UpdateCurrentTurn();
        }

        private void focusCharacterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(selectedCharacter == null || activeCharacter == selectedCharacter)
            {
                return;
            }

            focusedCharacter = selectedCharacter;
            UpdateFocusChar();
        }

        private void IntOnlyKeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
        }


    }
}
