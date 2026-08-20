namespace RickChess
{
    public partial class FormRickChess : Form
    {
        // Matriz para guardar informações de cada casa do tabuleiro
        private readonly CellInfo[,] boardCells = new CellInfo[8, 8];

        private class CellInfo
        {
            public Point TopLeft;
            public Point BottomRight;
            // Nome da peça: "peão", "rei", "dama", "torre", "cavalo", "bispo" ou "vazio"
            public string PieceName = "vazio";
            // Cor da peça (ex.: "branco", "preto" ou string vazia se sem peça)
            public string PieceColor = "";
            // Cached image for rendering (optional)
            public Image? PieceImage;
            // Threat markers
            public bool IsThreatened = false; // red marker on this cell when a piece here is attacked
            public List<Point> Attackers = new List<Point>(); // coordinates (r,c) of attackers that threaten this cell
            public bool IsAttacker = false; // green marker on attacker cell
        }

        // Create a deep snapshot of the current board state for undo purposes.
        private BoardSnapshot CloneBoardSnapshot()
        {
            var snap = new BoardSnapshot();
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    var src = boardCells[r, c];
                    var dst = new CellInfo();
                    if (src != null)
                    {
                        // copy only logical state; UI coordinates and cached images will be recalculated
                        dst.PieceName = src.PieceName ?? "vazio";
                        dst.PieceColor = src.PieceColor ?? "";
                        dst.PieceImage = null;
                        dst.IsThreatened = false;
                        dst.Attackers = new List<Point>();
                        dst.IsAttacker = false;
                    }
                    else
                    {
                        dst.PieceName = "vazio";
                        dst.PieceColor = "";
                        dst.PieceImage = null;
                        dst.Attackers = new List<Point>();
                    }
                    snap.Cells[r, c] = dst;
                }
            }

            // copy captured lists
            snap.CapturedWhite = new List<(string, string)>(capturedWhite);
            snap.CapturedBlack = new List<(string, string)>(capturedBlack);

            return snap;
        }

        private void PushUndoState()
        {
            // push a snapshot of current logical board state
            var snap = CloneBoardSnapshot();
            undoStack.Push(snap);
            // Any new move invalidates the redo history
            redoStack.Clear();
        }

        private void UndoLastMove()
        {
            if (undoStack.Count == 0)
                return;
            // before restoring previous state, save current state for redo
            var current = CloneBoardSnapshot();
            redoStack.Push(current);
            var snap = undoStack.Pop();

            // Apply snapshot into the existing boardCells array (boardCells is readonly)
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    if (boardCells[r, c] == null)
                        boardCells[r, c] = new CellInfo();

                    var s = snap.Cells[r, c];
                    if (s == null)
                    {
                        boardCells[r, c].PieceName = "vazio";
                        boardCells[r, c].PieceColor = "";
                        boardCells[r, c].PieceImage = null;
                    }
                    else
                    {
                        boardCells[r, c].PieceName = s.PieceName ?? "vazio";
                        boardCells[r, c].PieceColor = s.PieceColor ?? "";
                        boardCells[r, c].PieceImage = null;
                    }

                    // clear threat markers; RecalculateThreats will recompute them
                    boardCells[r, c].IsThreatened = false;
                    boardCells[r, c].Attackers.Clear();
                    boardCells[r, c].IsAttacker = false;
                }
            }

            // restore captured lists
            capturedWhite.Clear();
            capturedWhite.AddRange(snap.CapturedWhite);
            capturedBlack.Clear();
            capturedBlack.AddRange(snap.CapturedBlack);

            RecalculateThreats();
            UpdateCapturedPictureBoxes();
            picBoard.Invalidate();
        }

        private void RedoLastMove()
        {
            if (redoStack.Count == 0)
                return;

            // before restoring redo state, save current state so it can be undone
            var current = CloneBoardSnapshot();
            undoStack.Push(current);
            var snap = redoStack.Pop();

            // Apply snapshot into the existing boardCells array
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    if (boardCells[r, c] == null)
                        boardCells[r, c] = new CellInfo();

                    var s = snap.Cells[r, c];
                    if (s == null)
                    {
                        boardCells[r, c].PieceName = "vazio";
                        boardCells[r, c].PieceColor = "";
                        boardCells[r, c].PieceImage = null;
                    }
                    else
                    {
                        boardCells[r, c].PieceName = s.PieceName ?? "vazio";
                        boardCells[r, c].PieceColor = s.PieceColor ?? "";
                        boardCells[r, c].PieceImage = null;
                    }

                    // clear threat markers; RecalculateThreats will recompute them
                    boardCells[r, c].IsThreatened = false;
                    boardCells[r, c].Attackers.Clear();
                    boardCells[r, c].IsAttacker = false;
                }
            }

            // restore captured lists
            capturedWhite.Clear();
            capturedWhite.AddRange(snap.CapturedWhite);
            capturedBlack.Clear();
            capturedBlack.AddRange(snap.CapturedBlack);

            RecalculateThreats();
            UpdateCapturedPictureBoxes();
            picBoard.Invalidate();
        }

        // Return list of squares attacked by piece at (r,c) according to basic chess rules
        // Each Point is (row, col)
        private List<Point> GetAttackSquares(int r, int c, string pieceName, string pieceColor)
        {
            var res = new List<Point>();
            string p = pieceName.ToLower();
            if (p == "peão" || p == "peao")
            {
                // Pawns attack diagonally forward depending on color and board orientation
                // When the board is not flipped, white pawns move up (decreasing r) and black down (increasing r).
                // When the board is flipped, the visual/top-bottom is inverted so the pawn forward direction
                // should also invert relative to board coordinates.
                int dir;
                var colorLower = (pieceColor ?? "").ToLower();
                if (colorLower == "branco")
                    dir = isFlipped ? 1 : -1;
                else
                    dir = isFlipped ? -1 : 1;

                res.Add(new Point(r + dir, c - 1));
                res.Add(new Point(r + dir, c + 1));
                return res;
            }

            if (p == "cavalo")
            {
                int[] dr = { -2, -2, -1, -1, 1, 1, 2, 2 };
                int[] dc = { -1, 1, -2, 2, -2, 2, -1, 1 };
                for (int i = 0; i < dr.Length; i++) res.Add(new Point(r + dr[i], c + dc[i]));
                return res;
            }

            if (p == "rei")
            {
                for (int dr = -1; dr <= 1; dr++)
                    for (int dc = -1; dc <= 1; dc++)
                        if (dr != 0 || dc != 0)
                            res.Add(new Point(r + dr, c + dc));
                return res;
            }

            if (p == "bispo" || p == "torre" || p == "dama")
            {
                // sliding pieces
                var directions = new List<Point>();
                if (p == "bispo" || p == "dama")
                {
                    directions.Add(new Point(-1, -1));
                    directions.Add(new Point(-1, 1));
                    directions.Add(new Point(1, -1));
                    directions.Add(new Point(1, 1));
                }
                if (p == "torre" || p == "dama")
                {
                    directions.Add(new Point(-1, 0));
                    directions.Add(new Point(1, 0));
                    directions.Add(new Point(0, -1));
                    directions.Add(new Point(0, 1));
                }

                foreach (var d in directions)
                {
                    int tr = r + d.X;
                    int tc = c + d.Y;
                    while (tr >= 0 && tr < 8 && tc >= 0 && tc < 8)
                    {
                        res.Add(new Point(tr, tc));
                        var cell = boardCells[tr, tc];
                        if (cell != null && !string.IsNullOrEmpty(cell.PieceName) && cell.PieceName != "vazio")
                        {
                            // blocked after this square
                            break;
                        }
                        tr += d.X;
                        tc += d.Y;
                    }
                }
                return res;
            }

            return res;
        }

        // Whether the board is flipped vertically (true = inverted)
        private bool isFlipped = false;
        // Captured pieces (history) - store as tuple (Name, Color)
        private readonly List<(string Name, string Color)> capturedWhite = new List<(string, string)>();
        private readonly List<(string Name, string Color)> capturedBlack = new List<(string, string)>();

        // Undo/Redo stacks: store snapshots of the board so we can revert/restore moves
        private class BoardSnapshot
        {
            public CellInfo[,] Cells = new CellInfo[8, 8];
            public List<(string Name, string Color)> CapturedWhite = new List<(string, string)>();
            public List<(string Name, string Color)> CapturedBlack = new List<(string, string)>();
        }

        private readonly Stack<BoardSnapshot> undoStack = new Stack<BoardSnapshot>();
        private readonly Stack<BoardSnapshot> redoStack = new Stack<BoardSnapshot>();
        // Drag & drop state
        private bool isDragging = false;
        private int dragStartR = -1;
        private int dragStartC = -1;
        private string dragPieceName = "";
        private string dragPieceColor = "";
        private Image? dragPieceImage = null;
        private Point dragMousePos;

        // Desenha a peça presente na célula, usando as imagens em Properties.Resources
        private void DrawPieceIfPresent(Graphics g, CellInfo cell, Rectangle cellRect)
        {
            if (cell == null)
                return;
            if (string.IsNullOrEmpty(cell.PieceName) || cell.PieceName == "vazio")
                return;

            Image? img = cell.PieceImage ??= GetPieceImage(cell.PieceName, cell.PieceColor);
            if (img == null)
                return;

            // Compute destination rectangle, preserving aspect and leaving padding
            int padding = Math.Max(2, cellRect.Width / 10);
            var dest = new Rectangle(cellRect.Left + padding, cellRect.Top + padding, cellRect.Width - 2 * padding, cellRect.Height - 2 * padding);
            g.DrawImage(img, dest);
        }

        // Update the picture boxes that display captured pieces history
        private void UpdateCapturedPictureBoxes()
        {
            try
            {
                // Compute material value of a captured-pieces list
                int PieceListValue(List<(string Name, string Color)> list)
                {
                    int sum = 0;
                    foreach (var it in list)
                    {
                        var nm = (it.Name ?? "").ToLower();
                        if (nm == "peão" || nm == "peao") sum += 1;
                        else if (nm == "cavalo" || nm == "bispo") sum += 3;
                        else if (nm == "torre") sum += 5;
                        else if (nm == "dama") sum += 9;
                    }
                    return sum;
                }

                // Net score for each side = value of pieces it captured minus value of pieces it lost.
                // capturedBlack = black pieces captured (i.e. captured BY white); capturedWhite = white pieces captured (i.e. captured BY black)
                int blackPiecesValue = PieceListValue(capturedBlack);
                int whitePiecesValue = PieceListValue(capturedWhite);
                int netWhiteScore = blackPiecesValue - whitePiecesValue;
                int netBlackScore = whitePiecesValue - blackPiecesValue;

                // helper to render a list into a PictureBox
                void RenderListToPictureBox(List<(string Name, string Color)> list, PictureBox pic, int netScore)
                {
                    if (pic == null) return;
                    int w = Math.Max(1, pic.Width);
                    int h = Math.Max(1, pic.Height);

                    // If no items, clear image
                    if (list == null || list.Count == 0)
                    {
                        var old0 = pic.Image;
                        pic.Image = null;
                        old0?.Dispose();
                        return;
                    }

                    using var bmp = new Bitmap(w, h);
                    using var g = Graphics.FromImage(bmp);
                    // Use high quality scaling so thumbnails keep good appearance when reduced
                    g.Clear(pic.BackColor);
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                    int padding = 2;
                    int n = list.Count;

                    // Use a fixed thumbnail size (independent of form). Keep small but readable.
                    const int fixedThumb = 20; // pixels square (can be adjusted)
                    // Ensure thumbnail fits vertically; if PictureBox is shorter, reduce to available height.
                    int maxThumbByHeight = Math.Max(4, h - padding * 2);
                    int thumbW = Math.Min(fixedThumb, Math.Max(4, maxThumbByHeight));
                    int thumbH = Math.Min(thumbW, maxThumbByHeight);

                    int x = padding;
                    using (var scoreFont = new Font(SystemFonts.DefaultFont.FontFamily, 8f, FontStyle.Bold))
                    {
                        // score text color: white when rendering captured white pieces, black otherwise
                        var scoreColor = (pic == picCapturedWhite) ? Color.White : Color.FromArgb(220, Color.Black);
                        using var scoreBrush = new SolidBrush(scoreColor);
                        // only show a score when this side has a positive material advantage;
                        // a negative or zero net leaves the score blank for this color
                        string scoreText = netScore > 0 ? "+" + netScore.ToString() : "";
                        if (scoreText.Length > 0)
                        {
                            var textSize = g.MeasureString(scoreText, scoreFont);
                            // draw score at left, vertically centered
                            g.DrawString(scoreText, scoreFont, scoreBrush, new PointF(x, (h - textSize.Height) / 2f));
                            // advance x to leave space for score + gap
                            x += (int)Math.Ceiling(textSize.Width) + padding;
                        }

                        // draw thumbnails after score
                        for (int i = 0; i < n; i++)
                        {
                            var item = list[i];
                            var img = GetPieceImage(item.Name, item.Color);
                            if (img != null)
                            {
                                // destination box top-left (center thumbnails vertically inside picture box area)
                                int topBase = padding + (thumbH < (h - padding * 2) ? ((h - padding * 2 - thumbH) / 2) : 0);

                                // preserve aspect ratio of original image when scaling to fit thumbW x thumbH
                                double scale = Math.Min((double)thumbW / img.Width, (double)thumbH / img.Height);
                                int drawW = Math.Max(1, (int)Math.Round(img.Width * scale));
                                int drawH = Math.Max(1, (int)Math.Round(img.Height * scale));

                                int dx = (thumbW - drawW) / 2;
                                int dy = (thumbH - drawH) / 2;

                                var dest = new Rectangle(x + dx, topBase + dy, drawW, drawH);
                                g.DrawImage(img, dest);
                            }
                            x += thumbW + padding;
                        }
                    }

                    // replace image safely
                    var old = pic.Image;
                    pic.Image = (Bitmap)bmp.Clone();
                    old?.Dispose();
                }

                RenderListToPictureBox(capturedBlack, picCapturedBlack, netWhiteScore);
                RenderListToPictureBox(capturedWhite, picCapturedWhite, netBlackScore);
            }
            catch
            {
                // ignore rendering errors
            }
        }

        // After each move, recalculate threats for all pieces
        private void RecalculateThreats()
        {
            // Clear previous markers
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    var ci = boardCells[r, c];
                    if (ci == null) continue;
                    ci.IsThreatened = false;
                    ci.Attackers.Clear();
                    ci.IsAttacker = false;
                }
            }

            // For each piece, find what squares it attacks and mark target if contains opposite color piece
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    var src = boardCells[r, c];
                    if (src == null) continue;
                    if (string.IsNullOrEmpty(src.PieceName) || src.PieceName == "vazio") continue;

                    var attacks = GetAttackSquares(r, c, src.PieceName, src.PieceColor);
                    foreach (var pt in attacks)
                    {
                        int tr = pt.X;
                        int tc = pt.Y;
                        if (tr < 0 || tr >= 8 || tc < 0 || tc >= 8) continue;
                        var target = boardCells[tr, tc];
                        if (target == null) continue;
                        if (string.IsNullOrEmpty(target.PieceName) || target.PieceName == "vazio") continue;
                        if (!string.Equals(target.PieceColor, src.PieceColor, StringComparison.OrdinalIgnoreCase))
                        {
                            // src attacks target
                            target.IsThreatened = true;
                            target.Attackers.Add(new Point(r, c));
                            src.IsAttacker = true;
                        }
                    }
                }
            }
        }

        private Image? GetPieceImage(string pieceName, string pieceColor)
        {
            // Map Portuguese names to resource bitmaps
            return (pieceColor.ToLower(), pieceName.ToLower()) switch
            {
                ("branco", "peão") or ("branco", "peao") => Properties.Resources.whitePawn,
                ("branco", "rei") => Properties.Resources.whiteKing,
                ("branco", "dama") => Properties.Resources.whiteQueen,
                ("branco", "torre") => Properties.Resources.whiteRook,
                ("branco", "cavalo") => Properties.Resources.whiteKnight,
                ("branco", "bispo") => Properties.Resources.whiteBishop,

                ("preto", "peão") or ("preto", "peao") => Properties.Resources.blackPawn,
                ("preto", "rei") => Properties.Resources.blackKing,
                ("preto", "dama") => Properties.Resources.blackQueen,
                ("preto", "torre") => Properties.Resources.blackRook,
                ("preto", "cavalo") => Properties.Resources.blackKnight,
                ("preto", "bispo") => Properties.Resources.blackBishop,

                _ => null,
            };
        }

        public FormRickChess()
        {
            InitializeComponent();
            UpdateCapturedPanelOrder();
            InitializeBoardPieces();
            picBoard.Paint += PicBoard_Paint;
            this.Resize += FormRickChess_Resize;
            this.KeyPreview = true;
            this.KeyDown += FormRickChess_KeyDown;
            picBoard.MouseDown += PicBoard_MouseDown;
            picBoard.MouseMove += PicBoard_MouseMove;
            picBoard.MouseUp += PicBoard_MouseUp;
            picBoard.Invalidate();
        }

        private void FormRickChess_KeyDown(object? sender, KeyEventArgs e)
        {
            // Ctrl+Z = undo last move
            if (e.Control && e.KeyCode == Keys.Z)
            {
                UndoLastMove();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }

            // Ctrl+Y = redo
            if (e.Control && e.KeyCode == Keys.Y)
            {
                RedoLastMove();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode == Keys.F2)
            {
                InvertBoard();
            }

            if (e.KeyCode == Keys.F3)
            {
                ResetBoard();
            }
        }

        private void PicBoard_MouseDown(object? sender, MouseEventArgs e)
        {
            // Determine which cell was clicked
            var rect = picBoard.ClientRectangle;
            const int rows = 8;
            const int cols = 8;
            if (rect.Width <= 0 || rect.Height <= 0)
                return;
            int squareSize = Math.Min(rect.Width / cols, rect.Height / rows);

            int c = e.X / squareSize;
            int r = e.Y / squareSize;
            if (r < 0 || r >= 8 || c < 0 || c >= 8)
                return;

            if (boardCells[r, c] == null)
                return;

            var cell = boardCells[r, c];
            if (string.IsNullOrEmpty(cell.PieceName) || cell.PieceName == "vazio")
                return; // nothing to drag

            // Begin dragging
            isDragging = true;
            dragStartR = r;
            dragStartC = c;
            dragPieceName = cell.PieceName;
            dragPieceColor = cell.PieceColor;
            dragPieceImage = cell.PieceImage ?? GetPieceImage(cell.PieceName, cell.PieceColor);
            dragMousePos = e.Location;
            picBoard.Invalidate();
        }

        private void PicBoard_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!isDragging)
                return;
            dragMousePos = e.Location;
            picBoard.Invalidate();
        }

        private void PicBoard_MouseUp(object? sender, MouseEventArgs e)
        {
            if (!isDragging)
                return;

            // Determine destination cell
            var rect = picBoard.ClientRectangle;
            const int rows = 8;
            const int cols = 8;
            if (rect.Width <= 0 || rect.Height <= 0)
            {
                CancelDrag();
                return;
            }
            int squareSize = Math.Min(rect.Width / cols, rect.Height / rows);
            int c = e.X / squareSize;
            int r = e.Y / squareSize;
            if (r < 0 || r >= 8 || c < 0 || c >= 8)
            {
                // released outside board -> cancel drag
                CancelDrag();
                return;
            }

            // If destination is same as origin, cancel
            if (r == dragStartR && c == dragStartC)
            {
                CancelDrag();
                return;
            }

            var srcCell = boardCells[dragStartR, dragStartC];
            var dstCell = boardCells[r, c];
            if (srcCell == null || dstCell == null)
            {
                CancelDrag();
                return;
            }

            // Bishop color rule: bishops remain on same square color. If a bishop is moved to a square
            // with different parity (light/dark), cancel the move and keep board state.
            var srcNameLower = (srcCell.PieceName ?? "").ToLower();
            if (srcNameLower == "bispo")
            {
                int srcParity = (dragStartR + dragStartC) & 1;
                int dstParity = (r + c) & 1;
                if (srcParity != dstParity)
                {
                    // invalid bishop destination: cancel move and keep state
                    CancelDrag();
                    picBoard.Invalidate();
                    return;
                }
            }

            // If destination empty -> move
            if (string.IsNullOrEmpty(dstCell.PieceName) || dstCell.PieceName == "vazio")
            {
                // record state before making the move so it can be undone
                PushUndoState();

                dstCell.PieceName = srcCell.PieceName;
                dstCell.PieceColor = srcCell.PieceColor;
                dstCell.PieceImage = null; // will be reloaded when painting

                srcCell.PieceName = "vazio";
                srcCell.PieceColor = "";
                srcCell.PieceImage = null;
                PlayMoveSound();
                RecalculateThreats();
                CancelDrag();
                picBoard.Invalidate();
                return;
            }

            // If destination has piece: check colors
            if (!string.Equals(srcCell.PieceColor, dstCell.PieceColor, StringComparison.OrdinalIgnoreCase))
            {
                // record captured piece info before changing state
                var capturedName = dstCell.PieceName;
                var capturedColor = dstCell.PieceColor;

                // record state before capture so it can be undone
                PushUndoState();

                // Capture: replace destination with source piece, clear source
                dstCell.PieceName = srcCell.PieceName;
                dstCell.PieceColor = srcCell.PieceColor;
                dstCell.PieceImage = null;

                srcCell.PieceName = "vazio";
                srcCell.PieceColor = "";
                srcCell.PieceImage = null;

                // Add to captured lists (history)
                if (!string.IsNullOrEmpty(capturedName) && capturedName != "vazio")
                {
                    if ((capturedColor ?? "").ToLower() == "branco")
                        capturedWhite.Add((capturedName, capturedColor ?? ""));
                    else
                        capturedBlack.Add((capturedName, capturedColor ?? ""));
                    UpdateCapturedPictureBoxes();
                }

                PlayMoveSound();
                RecalculateThreats();
                CancelDrag();
                picBoard.Invalidate();
                return;
            }

            // Same color -> cancel move
            CancelDrag();
            picBoard.Invalidate();
        }

        private void CancelDrag()
        {
            isDragging = false;
            dragStartR = -1;
            dragStartC = -1;
            dragPieceName = "";
            dragPieceColor = "";
            dragPieceImage = null;
        }

        // Play move/capture sound from embedded resource (movePiece.mp3)
        private void PlayMoveSound()
        {
            try
            {
                // write resource stream to temp file if missing
                var tmp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "RickChess_movePiece.mp3");
                if (!System.IO.File.Exists(tmp))
                {
                    using var s = Properties.Resources.movePiece;
                    if (s != null)
                    {
                        using var fs = System.IO.File.Create(tmp);
                        s.CopyTo(fs);
                        fs.Flush();
                    }
                }

                // Play via Windows Media Player COM object dynamically so no compile-time reference needed
                var progId = "WMPlayer.OCX";
                var t = Type.GetTypeFromProgID(progId);
                if (t != null && System.IO.File.Exists(tmp))
                {
                    dynamic player = Activator.CreateInstance(t);
                    player.URL = tmp;
                    // play asynchronously
                    player.controls.play();
                    // do not release here; let player manage playback. It will be collected later.
                }
            }
            catch
            {
                // ignore failures to play sound
            }
        }

        // Reorders the captured-pieces boxes so the box matching the color sitting
        // at the bottom of the board is also shown at the bottom of the right panel.
        // Uses explicit Top/Bottom docks (instead of stacking two Dock=Top controls)
        // so the placement doesn't depend on ambiguous z-order/add-order behavior.
        private void UpdateCapturedPanelOrder()
        {
            pnlCaptured.SuspendLayout();
            if (isFlipped)
            {
                // black sits at the bottom of the board -> show captured white pieces at the bottom
                picCapturedWhite.Dock = DockStyle.Bottom;
                picCapturedBlack.Dock = DockStyle.Top;
            }
            else
            {
                // white sits at the bottom of the board -> show captured black pieces at the bottom
                picCapturedBlack.Dock = DockStyle.Bottom;
                picCapturedWhite.Dock = DockStyle.Top;
            }
            pnlCaptured.ResumeLayout(true);
        }

        private void FlipBoard()
        {
            // Toggle flip flag
            isFlipped = !isFlipped;

            // Keep the captured-pieces panel order in sync with the new board orientation
            UpdateCapturedPanelOrder();

            // Rotate the board 180 degrees so pieces and coordinates invert consistently.
            var newBoard = new CellInfo[8, 8];
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    var src = boardCells[r, c];
                    if (src == null)
                        continue;
                    int tr = 7 - r;
                    int tc = 7 - c;
                    if (newBoard[tr, tc] == null) newBoard[tr, tc] = new CellInfo();
                    newBoard[tr, tc].PieceName = src.PieceName;
                    newBoard[tr, tc].PieceColor = src.PieceColor;
                    // clear cached image so GetPieceImage will be used fresh
                    newBoard[tr, tc].PieceImage = null;
                }
            }

            // Replace boardCells with rotated board
            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                    boardCells[r, c] = newBoard[r, c] ?? new CellInfo();

            // Recalculate threat markers after flipping so indicators follow the new board orientation
            RecalculateThreats();
        }

        // Configura a posição inicial das peças no tabuleiro
        private void InitializeBoardPieces()
        {
            // Ensure all cells exist
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    if (boardCells[r, c] == null)
                        boardCells[r, c] = new CellInfo();
                    boardCells[r, c].PieceName = "vazio";
                    boardCells[r, c].PieceColor = "";
                }
            }

            // Black back rank (top)
            SetCell(0, 0, "torre", "preto");
            SetCell(0, 1, "cavalo", "preto");
            SetCell(0, 2, "bispo", "preto");
            SetCell(0, 3, "dama", "preto");
            SetCell(0, 4, "rei", "preto");
            SetCell(0, 5, "bispo", "preto");
            SetCell(0, 6, "cavalo", "preto");
            SetCell(0, 7, "torre", "preto");

            // Black pawns
            for (int c = 0; c < 8; c++)
                SetCell(1, c, "peão", "preto");

            // White pawns
            for (int c = 0; c < 8; c++)
                SetCell(6, c, "peão", "branco");

            // White back rank (bottom)
            SetCell(7, 0, "torre", "branco");
            SetCell(7, 1, "cavalo", "branco");
            SetCell(7, 2, "bispo", "branco");
            SetCell(7, 3, "dama", "branco");
            SetCell(7, 4, "rei", "branco");
            SetCell(7, 5, "bispo", "branco");
            SetCell(7, 6, "cavalo", "branco");
            SetCell(7, 7, "torre", "branco");

            // Ensure threat markers are correct and record initial state for undo
            RecalculateThreats();
            // clear captured history and undo/redo stacks
            capturedWhite.Clear();
            capturedBlack.Clear();
            UpdateCapturedPictureBoxes();

            undoStack.Clear();
            redoStack.Clear();
            PushUndoState();
        }

        private void SetCell(int r, int c, string pieceName, string pieceColor)
        {
            if (boardCells[r, c] == null)
                boardCells[r, c] = new CellInfo();
            boardCells[r, c].PieceName = pieceName;
            boardCells[r, c].PieceColor = pieceColor;
        }

        private void FormRickChess_Resize(object? sender, EventArgs e)
        {
            // Force the picture box to repaint so the chessboard scales
            picBoard.Invalidate();
        }

        private void PicBoard_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var rect = picBoard.ClientRectangle;

            // Clear with the control background
            g.Clear(picBoard.BackColor);

            const int rows = 8;
            const int cols = 8;

            if (rect.Width <= 0 || rect.Height <= 0)
                return;

            // Use the largest square size that fits in the PictureBox
            int squareSize = Math.Min(rect.Width / cols, rect.Height / rows);
            int boardWidth = squareSize * cols;
            int boardHeight = squareSize * rows;

            // Draw the board starting at the top-left corner (0,0)
            int offsetX = 0;
            int offsetY = 0;

            // Dark green and light beige tones for the squares
            using var brushLight = new SolidBrush(Color.FromArgb(245, 245, 220)); // light beige
            using var brushDark = new SolidBrush(Color.FromArgb(34, 139, 34)); // dark green

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var brush = ((r + c) % 2 == 0) ? brushLight : brushDark;
                    var cellRect = new Rectangle(offsetX + c * squareSize, offsetY + r * squareSize, squareSize, squareSize);
                    g.FillRectangle(brush, cellRect);

                    // Atualiza a estrutura da célula com as coordenadas
                    if (boardCells[r, c] == null)
                        boardCells[r, c] = new CellInfo();

                    boardCells[r, c].TopLeft = new Point(cellRect.Left, cellRect.Top);
                    boardCells[r, c].BottomRight = new Point(cellRect.Right, cellRect.Bottom);
                    // Se não houver peça definida, manter como "vazio" e cor vazia
                    if (string.IsNullOrEmpty(boardCells[r, c].PieceName))
                        boardCells[r, c].PieceName = "vazio";
                    if (boardCells[r, c].PieceColor == null)
                        boardCells[r, c].PieceColor = "";

                    // Draw piece image from resources if present
                    DrawPieceIfPresent(g, boardCells[r, c], cellRect);

                    // If currently dragging this piece from its origin, don't draw it in the origin cell
                    if (isDragging && dragStartR == r && dragStartC == c)
                    {
                        // Overpaint the origin with the square color to hide the original while dragging
                        g.FillRectangle(brush, cellRect);
                    }

                    // Draw small discreet coordinates: row numbers on the first column and
                    // column letters on the last row, in the corner of the respective cells.
                    const int fontSize = 9;
                    using var font = new Font(SystemFonts.DefaultFont.FontFamily, fontSize, FontStyle.Regular);

                    // Choose a label color that contrasts with the square
                    var labelColor = (((r + c) % 2 == 0) ? Color.FromArgb(200, Color.Black) : Color.FromArgb(220, Color.White));
                    using var labelBrush = new SolidBrush(labelColor);

                    int padding = Math.Max(2, cellRect.Width / 20);

                    // Row numbers on the first (leftmost) column
                    if (c == 0)
                    {
                        // When white pieces are at bottom (isFlipped == false), row 1 is at the bottom -> label = 8 - r
                        // When board is flipped (white at top), row 1 is at the top -> label = r + 1
                        string rowLabel = isFlipped ? (r + 1).ToString() : (8 - r).ToString();
                        var rowPos = new PointF(cellRect.Left + padding, cellRect.Top + padding);
                        g.DrawString(rowLabel, font, labelBrush, rowPos);
                    }

                    // Column letters on the last (bottom) row, in lowercase and aligned to the baseline
                    if (r == rows - 1)
                    {
                        int index = isFlipped ? (7 - c) : c;
                        char letter = (char)('a' + index);
                        string colLabel = letter.ToString();
                        // measure to right-align inside the cell padding and place on baseline
                        var size = g.MeasureString(colLabel, font);
                        var colPos = new PointF(cellRect.Right - padding - size.Width, cellRect.Bottom - padding - size.Height);
                        g.DrawString(colLabel, font, labelBrush, colPos);
                    }

                    // Draw threat markers: red small circle on threatened piece, green on attackers
                    var ci = boardCells[r, c];
                    if (ci != null)
                    {
                        int markerRadius = Math.Max(4, cellRect.Width / 12);
                        if (ci.IsThreatened)
                        {
                            using var brushR = new SolidBrush(Color.FromArgb(200, Color.Red));
                            var markRect = new Rectangle(cellRect.Right - padding - markerRadius * 2, cellRect.Top + padding, markerRadius * 2, markerRadius * 2);
                            g.FillEllipse(brushR, markRect);
                        }
                        if (ci.IsAttacker)
                        {
                            using var brushG = new SolidBrush(Color.FromArgb(200, Color.Lime));
                            var markRect = new Rectangle(cellRect.Left + padding, cellRect.Bottom - padding - markerRadius * 2, markerRadius * 2, markerRadius * 2);
                            g.FillEllipse(brushG, markRect);
                        }
                    }
                }
            }

            // Draw a soft border around the board
            using var pen = new Pen(Color.LightGray, 2);
            g.DrawRectangle(pen, offsetX, offsetY, boardWidth, boardHeight);

            // Draw dragged piece following the mouse
            if (isDragging && dragPieceImage != null)
            {
                int padding = Math.Max(2, squareSize / 10);
                var dest = new Rectangle(dragMousePos.X - (squareSize - 2 * padding) / 2, dragMousePos.Y - (squareSize - 2 * padding) / 2, squareSize - 2 * padding, squareSize - 2 * padding);
                g.DrawImage(dragPieceImage, dest);
            }
        }

        private void InvertBoard()
        {
            FlipBoard();
            picBoard.Invalidate();
        }

        private void ResetBoard()
        {
            var result = MessageBox.Show("Are you sure you want to reset?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;
            InitializeBoardPieces();
            RecalculateThreats();
            picBoard.Invalidate();
        }

        private void Quit()
        {
            var result = MessageBox.Show("Are you sure you want to exit?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;
            this.Close();
        }

        private void InvertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InvertBoard();
        }

        private void QuitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Quit();
        }

        private void ResetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetBoard();
        }

        private void UndoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UndoLastMove();
        }

        private void RedoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RedoLastMove();
        }
    }
}
