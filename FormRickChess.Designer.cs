namespace RickChess
{
    partial class FormRickChess
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormRickChess));
            pnlCaptured = new Panel();
            picCapturedBlack = new PictureBox();
            picCapturedWhite = new PictureBox();
            picBoard = new PictureBox();
            menuChess = new MenuStrip();
            chessToolStripMenuItem = new ToolStripMenuItem();
            invertToolStripMenuItem = new ToolStripMenuItem();
            resetToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem1 = new ToolStripSeparator();
            undoLastMoveToolStripMenuItem = new ToolStripMenuItem();
            redoLastMoveToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem2 = new ToolStripSeparator();
            quitToolStripMenuItem = new ToolStripMenuItem();
            pnlCaptured.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picCapturedBlack).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picCapturedWhite).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picBoard).BeginInit();
            menuChess.SuspendLayout();
            SuspendLayout();
            // 
            // pnlCaptured
            // 
            pnlCaptured.Controls.Add(picCapturedBlack);
            pnlCaptured.Controls.Add(picCapturedWhite);
            pnlCaptured.Dock = DockStyle.Right;
            pnlCaptured.Location = new Point(603, 24);
            pnlCaptured.Name = "pnlCaptured";
            pnlCaptured.Size = new Size(418, 602);
            pnlCaptured.TabIndex = 0;
            // 
            // picCapturedBlack
            // 
            picCapturedBlack.Dock = DockStyle.Top;
            picCapturedBlack.Location = new Point(0, 70);
            picCapturedBlack.Name = "picCapturedBlack";
            picCapturedBlack.Size = new Size(418, 70);
            picCapturedBlack.TabIndex = 1;
            picCapturedBlack.TabStop = false;
            // 
            // picCapturedWhite
            // 
            picCapturedWhite.BackColor = Color.Gray;
            picCapturedWhite.Dock = DockStyle.Top;
            picCapturedWhite.Location = new Point(0, 0);
            picCapturedWhite.Name = "picCapturedWhite";
            picCapturedWhite.Size = new Size(418, 70);
            picCapturedWhite.TabIndex = 0;
            picCapturedWhite.TabStop = false;
            // 
            // picBoard
            // 
            picBoard.Dock = DockStyle.Fill;
            picBoard.Location = new Point(0, 24);
            picBoard.Name = "picBoard";
            picBoard.Size = new Size(603, 602);
            picBoard.TabIndex = 1;
            picBoard.TabStop = false;
            // 
            // menuChess
            // 
            menuChess.Items.AddRange(new ToolStripItem[] { chessToolStripMenuItem });
            menuChess.Location = new Point(0, 0);
            menuChess.Name = "menuChess";
            menuChess.Size = new Size(1021, 24);
            menuChess.TabIndex = 2;
            menuChess.Text = "menuStrip1";
            // 
            // chessToolStripMenuItem
            // 
            chessToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { invertToolStripMenuItem, resetToolStripMenuItem, toolStripMenuItem1, undoLastMoveToolStripMenuItem, redoLastMoveToolStripMenuItem, toolStripMenuItem2, quitToolStripMenuItem });
            chessToolStripMenuItem.Name = "chessToolStripMenuItem";
            chessToolStripMenuItem.Size = new Size(50, 20);
            chessToolStripMenuItem.Text = "Chess";
            // 
            // invertToolStripMenuItem
            // 
            invertToolStripMenuItem.Name = "invertToolStripMenuItem";
            invertToolStripMenuItem.ShortcutKeyDisplayString = "F2";
            invertToolStripMenuItem.Size = new Size(175, 22);
            invertToolStripMenuItem.Text = "Invert";
            invertToolStripMenuItem.Click += InvertToolStripMenuItem_Click;
            // 
            // resetToolStripMenuItem
            // 
            resetToolStripMenuItem.Name = "resetToolStripMenuItem";
            resetToolStripMenuItem.ShortcutKeyDisplayString = "F3";
            resetToolStripMenuItem.Size = new Size(175, 22);
            resetToolStripMenuItem.Text = "Reset";
            resetToolStripMenuItem.Click += ResetToolStripMenuItem_Click;
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(172, 6);
            // 
            // undoLastMoveToolStripMenuItem
            // 
            undoLastMoveToolStripMenuItem.Name = "undoLastMoveToolStripMenuItem";
            undoLastMoveToolStripMenuItem.ShortcutKeyDisplayString = "[CTRL] + [Z]";
            undoLastMoveToolStripMenuItem.Size = new Size(175, 22);
            undoLastMoveToolStripMenuItem.Text = "Undo";
            undoLastMoveToolStripMenuItem.Click += UndoToolStripMenuItem_Click;
            // 
            // redoLastMoveToolStripMenuItem
            // 
            redoLastMoveToolStripMenuItem.Name = "redoLastMoveToolStripMenuItem";
            redoLastMoveToolStripMenuItem.ShortcutKeyDisplayString = "[CTRL] + [Y]";
            redoLastMoveToolStripMenuItem.Size = new Size(175, 22);
            redoLastMoveToolStripMenuItem.Text = "Redo";
            redoLastMoveToolStripMenuItem.Click += RedoToolStripMenuItem_Click;
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new Size(172, 6);
            // 
            // quitToolStripMenuItem
            // 
            quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            quitToolStripMenuItem.Size = new Size(175, 22);
            quitToolStripMenuItem.Text = "Quit";
            quitToolStripMenuItem.Click += QuitToolStripMenuItem_Click;
            // 
            // FormRickChess
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Gray;
            ClientSize = new Size(1021, 626);
            Controls.Add(picBoard);
            Controls.Add(pnlCaptured);
            Controls.Add(menuChess);
            ForeColor = Color.Black;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "FormRickChess";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Rick Chess";
            pnlCaptured.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picCapturedBlack).EndInit();
            ((System.ComponentModel.ISupportInitialize)picCapturedWhite).EndInit();
            ((System.ComponentModel.ISupportInitialize)picBoard).EndInit();
            menuChess.ResumeLayout(false);
            menuChess.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel pnlCaptured;
        private PictureBox picBoard;
        private MenuStrip menuChess;
        private ToolStripMenuItem chessToolStripMenuItem;
        private ToolStripMenuItem invertToolStripMenuItem;
        private ToolStripMenuItem quitToolStripMenuItem;
        private ToolStripMenuItem resetToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripSeparator toolStripMenuItem2;
        private ToolStripMenuItem undoLastMoveToolStripMenuItem;
        private ToolStripMenuItem redoLastMoveToolStripMenuItem;
        private PictureBox picCapturedBlack;
        private PictureBox picCapturedWhite;
    }
}
