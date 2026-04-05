using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace TextEditor
{
    public partial class MainForm : Form
    {
        private TextBox textBoxEditor;
        private DataGridView dataGridViewResults;
        private MenuStrip menuStrip;
        private ToolStrip toolStrip;
        private SplitContainer splitContainer;
        private Scanner _scanner;
        private List<Token> _lastTokens;
        private string currentFilePath;
        private Stack<string> undoStack = new Stack<string>();
        private Stack<string> redoStack = new Stack<string>();
        private bool isUndoRedoOperation = false;

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Текстовый редактор - Языковой процессор";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(500, 400);

            dataGridViewResults = new DataGridView();
            dataGridViewResults.Dock = DockStyle.Fill;
            dataGridViewResults.ReadOnly = true;
            dataGridViewResults.AllowUserToAddRows = false;
            dataGridViewResults.AllowUserToDeleteRows = false;
            dataGridViewResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewResults.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewResults.CellClick += DataGridViewResults_CellClick;
            dataGridViewResults.Columns.Add("Fragment", "Неверный фрагмент");
            dataGridViewResults.Columns.Add("Location", "Местоположение");
            dataGridViewResults.Columns.Add("Description", "Описание ошибки");

            splitContainer = new SplitContainer();
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Orientation = Orientation.Horizontal;
            splitContainer.SplitterDistance = this.ClientSize.Height * 2 / 3;
            splitContainer.SplitterWidth = 5;
            splitContainer.Panel1MinSize = 100;
            splitContainer.Panel2MinSize = 100;

            textBoxEditor = new TextBox();
            textBoxEditor.Multiline = true;
            textBoxEditor.ScrollBars = ScrollBars.Both;
            textBoxEditor.Dock = DockStyle.Fill;
            textBoxEditor.Font = new Font("Consolas", 10);
            textBoxEditor.AcceptsTab = true;
            textBoxEditor.WordWrap = false;
            textBoxEditor.TextChanged += TextBoxEditor_TextChanged;

            splitContainer.Panel1.Controls.Add(textBoxEditor);
            splitContainer.Panel2.Controls.Add(dataGridViewResults);

            menuStrip = new MenuStrip();
            menuStrip.Dock = DockStyle.Top;

            ToolStripMenuItem fileMenu = new ToolStripMenuItem("Файл");
            fileMenu.DropDownItems.Add("Создать", null, OnFileNew);
            fileMenu.DropDownItems.Add("Открыть", null, OnFileOpen);
            fileMenu.DropDownItems.Add("Сохранить", null, OnFileSave);
            fileMenu.DropDownItems.Add("Сохранить как", null, OnFileSaveAs);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("Выход", null, OnFileExit);

            ToolStripMenuItem editMenu = new ToolStripMenuItem("Правка");
            editMenu.DropDownItems.Add("Отменить", null, OnEditUndo);
            editMenu.DropDownItems.Add("Повторить", null, OnEditRedo);
            editMenu.DropDownItems.Add(new ToolStripSeparator());
            editMenu.DropDownItems.Add("Вырезать", null, OnEditCut);
            editMenu.DropDownItems.Add("Копировать", null, OnEditCopy);
            editMenu.DropDownItems.Add("Вставить", null, OnEditPaste);
            editMenu.DropDownItems.Add("Удалить", null, OnEditDelete);
            editMenu.DropDownItems.Add(new ToolStripSeparator());
            editMenu.DropDownItems.Add("Выделить все", null, OnEditSelectAll);

            ToolStripMenuItem textMenu = new ToolStripMenuItem("Текст");
            textMenu.DropDownItems.Add("Постановка задачи", null, OnTextInfo);
            textMenu.DropDownItems.Add("Грамматика", null, OnTextInfo);
            textMenu.DropDownItems.Add("Классификация грамматики", null, OnTextInfo);
            textMenu.DropDownItems.Add("Метод анализа", null, OnTextInfo);
            textMenu.DropDownItems.Add("Тестовый пример", null, OnTextInfo);
            textMenu.DropDownItems.Add("Список литературы", null, OnTextInfo);
            textMenu.DropDownItems.Add("Исходный код программы", null, OnTextInfo);

            ToolStripMenuItem startMenu = new ToolStripMenuItem("Пуск");
            startMenu.Click += OnStartAnalysis;

            ToolStripMenuItem helpMenu = new ToolStripMenuItem("Справка");
            helpMenu.DropDownItems.Add("Вызов справки", null, OnHelp);
            helpMenu.DropDownItems.Add("О программе", null, OnAbout);

            menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, editMenu, textMenu, startMenu, helpMenu });

            toolStrip = new ToolStrip();
            toolStrip.Dock = DockStyle.Top;
            toolStrip.ImageScalingSize = new Size(20, 20);

            toolStrip.Items.Add(new ToolStripButton("📄", null, OnFileNew, "New"));
            toolStrip.Items.Add(new ToolStripButton("📂", null, OnFileOpen, "Open"));
            toolStrip.Items.Add(new ToolStripButton("💾", null, OnFileSave, "Save"));
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(new ToolStripButton("↶", null, OnEditUndo, "Undo"));
            toolStrip.Items.Add(new ToolStripButton("↷", null, OnEditRedo, "Redo"));
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(new ToolStripButton("✂️", null, OnEditCut, "Cut"));
            toolStrip.Items.Add(new ToolStripButton("📋", null, OnEditCopy, "Copy"));
            toolStrip.Items.Add(new ToolStripButton("📝", null, OnEditPaste, "Paste"));
            toolStrip.Items.Add(new ToolStripButton("❌", null, OnEditDelete, "Delete"));
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(new ToolStripButton("▶️", null, OnStartAnalysis, "Start"));
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(new ToolStripButton("🔍", null, OnHelp, "Help"));
            toolStrip.Items.Add(new ToolStripButton("ℹ️", null, OnAbout, "About"));

            this.Controls.Add(splitContainer);
            this.Controls.Add(toolStrip);
            this.Controls.Add(menuStrip);
            this.MainMenuStrip = menuStrip;

            this.Resize += MainForm_Resize;

            _scanner = new Scanner();
            SaveState();
        }

        private void TextBoxEditor_TextChanged(object sender, EventArgs e)
        {
            if (!isUndoRedoOperation)
            {
                SaveState();
            }
        }

        private void SaveState()
        {
            undoStack.Push(textBoxEditor.Text);
            redoStack.Clear();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (this.WindowState != FormWindowState.Minimized)
            {
                splitContainer.SplitterDistance = Math.Max(100, Math.Min(splitContainer.Height - 100, this.ClientSize.Height * 2 / 3));
            }
        }

        private void OnFileNew(object sender, EventArgs e)
        {
            if (ConfirmSaveChanges())
            {
                textBoxEditor.Clear();
                dataGridViewResults.Rows.Clear();
                _lastTokens = null;
                currentFilePath = null;
                undoStack.Clear();
                redoStack.Clear();
                SaveState();
                this.Text = "Текстовый редактор - Языковой процессор [Новый файл]";
            }
        }

        private void OnFileOpen(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (ConfirmSaveChanges())
                {
                    textBoxEditor.Text = System.IO.File.ReadAllText(dialog.FileName);
                    currentFilePath = dialog.FileName;
                    undoStack.Clear();
                    redoStack.Clear();
                    SaveState();
                    this.Text = $"Текстовый редактор - Языковой процессор [{dialog.FileName}]";
                }
            }
        }

        private void OnFileSave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                OnFileSaveAs(sender, e);
                return;
            }
            try
            {
                System.IO.File.WriteAllText(currentFilePath, textBoxEditor.Text);
                this.Text = $"Текстовый редактор - Языковой процессор [{currentFilePath}]";
                MessageBox.Show("Файл сохранен успешно!", "Сохранение", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnFileSaveAs(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    System.IO.File.WriteAllText(dialog.FileName, textBoxEditor.Text);
                    currentFilePath = dialog.FileName;
                    this.Text = $"Текстовый редактор - Языковой процессор [{dialog.FileName}]";
                    MessageBox.Show("Файл сохранен успешно!", "Сохранение", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OnFileExit(object sender, EventArgs e)
        {
            this.Close();
        }

        private bool ConfirmSaveChanges()
        {
            if (!string.IsNullOrEmpty(textBoxEditor.Text))
            {
                DialogResult result = MessageBox.Show("Сохранить изменения в текущем файле?", "Подтверждение", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    OnFileSave(this, EventArgs.Empty);
                    return true;
                }
                else if (result == DialogResult.No)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        private void OnEditUndo(object sender, EventArgs e)
        {
            if (undoStack.Count > 1)
            {
                isUndoRedoOperation = true;
                string currentState = undoStack.Pop();
                redoStack.Push(currentState);
                string previousState = undoStack.Peek();
                textBoxEditor.Text = previousState;
                textBoxEditor.SelectionStart = textBoxEditor.Text.Length;
                textBoxEditor.SelectionLength = 0;
                isUndoRedoOperation = false;
            }
        }

        private void OnEditRedo(object sender, EventArgs e)
        {
            if (redoStack.Count > 0)
            {
                isUndoRedoOperation = true;
                string stateToRedo = redoStack.Pop();
                undoStack.Push(stateToRedo);
                textBoxEditor.Text = stateToRedo;
                textBoxEditor.SelectionStart = textBoxEditor.Text.Length;
                textBoxEditor.SelectionLength = 0;
                isUndoRedoOperation = false;
            }
        }

        private void OnEditCut(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBoxEditor.SelectedText))
            {
                textBoxEditor.Cut();
                SaveState();
            }
        }

        private void OnEditCopy(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBoxEditor.SelectedText))
            {
                textBoxEditor.Copy();
            }
        }

        private void OnEditPaste(object sender, EventArgs e)
        {
            textBoxEditor.Paste();
            SaveState();
        }

        private void OnEditDelete(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBoxEditor.SelectedText))
            {
                int selectionStart = textBoxEditor.SelectionStart;
                textBoxEditor.Text = textBoxEditor.Text.Remove(textBoxEditor.SelectionStart, textBoxEditor.SelectionLength);
                textBoxEditor.SelectionStart = selectionStart;
                SaveState();
            }
        }

        private void OnEditSelectAll(object sender, EventArgs e) => textBoxEditor.SelectAll();

        private void OnTextInfo(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            string info = item.Text;
            MessageBox.Show($"Информация по пункту \"{info}\" будет добавлена позже.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnStartAnalysis(object sender, EventArgs e)
        {
            string inputText = textBoxEditor.Text;

            var (tokens, lexErrors) = _scanner.Scan(inputText);
            _lastTokens = tokens;

            dataGridViewResults.Rows.Clear();

            if (lexErrors.Count > 0)
            {
                foreach (var errorToken in lexErrors)
                {
                    int rowIndex = dataGridViewResults.Rows.Add(
                        errorToken.Value,
                        errorToken.Position,
                        $"Лексическая ошибка: {errorToken.Type}"
                    );
                    var row = dataGridViewResults.Rows[rowIndex];
                    row.Tag = errorToken;
                    row.DefaultCellStyle.BackColor = Color.LightCoral;
                }
                MessageBox.Show($"Обнаружено лексических ошибок: {lexErrors.Count}", "Результат анализа",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Parser parser = new Parser();
            List<SyntaxError> syntaxErrors = parser.Parse(tokens, inputText);

            if (syntaxErrors.Count == 0)
            {
                dataGridViewResults.Rows.Add("", "", "Синтаксических ошибок не найдено.");
                MessageBox.Show("Синтаксических ошибок не найдено.", "Результат анализа",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                foreach (var error in syntaxErrors)
                {
                    int rowIndex = dataGridViewResults.Rows.Add(
                        error.Fragment,
                        error.Location,
                        error.Description
                    );
                    var row = dataGridViewResults.Rows[rowIndex];
                    row.Tag = error;
                    row.DefaultCellStyle.BackColor = Color.LightCoral;
                }
                MessageBox.Show($"Обнаружено синтаксических ошибок: {syntaxErrors.Count}", "Результат анализа",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DataGridViewResults_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var row = dataGridViewResults.Rows[e.RowIndex];

                if (row.Tag is SyntaxError syntaxError)
                {
                    if (syntaxError.CharPosition >= 0)
                    {
                        textBoxEditor.Focus();
                        textBoxEditor.Select(syntaxError.CharPosition, 0);
                        textBoxEditor.ScrollToCaret();
                    }
                }
                else if (row.Tag is Token token)
                {
                    textBoxEditor.Focus();
                    textBoxEditor.Select(token.StartPos, token.Value.Length);
                    textBoxEditor.ScrollToCaret();
                }
            }
        }

        private void OnHelp(object sender, EventArgs e)
        {
            string helpText = @"ТЕКСТОВЫЙ РЕДАКТОР - ЯЗЫКОВОЙ ПРОЦЕССОР

СПРАВКА ПО ФУНКЦИЯМ ПРОГРАММЫ
===========================================

МЕНЮ ""ФАЙЛ""
===========================================
- Создать - создает новый пустой документ
- Открыть - открывает существующий текстовый файл
- Сохранить - сохраняет текущий документ
- Сохранить как - сохраняет документ с новым именем
- Выход - закрывает программу

МЕНЮ ""ПРАВКА""
===========================================
- Отменить - отменяет последнее действие
- Повторить - повторяет отмененное действие
- Вырезать - удаляет выделенный текст и копирует его в буфер
- Копировать - копирует выделенный текст в буфер
- Вставить - вставляет текст из буфера
- Удалить - удаляет выделенный текст
- Выделить все - выделяет весь текст

МЕНЮ ""ТЕКСТ""
===========================================
Содержит информацию по курсовой работе:
- Постановка задачи
- Грамматика
- Классификация грамматики
- Метод анализа
- Диагностика и нейтрализация ошибок
- Тестовый пример
- Список литературы
- Исходный код программы

МЕНЮ ""ПУСК""
===========================================
Запускает лексический и синтаксический анализ текста.
Результаты отображаются в нижней панели.

МЕНЮ ""СПРАВКА""
===========================================
- Вызов справки - открывает данное окно справки
- О программе - показывает информацию о версии

ПАНЕЛЬ ИНСТРУМЕНТОВ
===========================================
Содержит кнопки быстрого доступа к часто используемым функциям.

РАБОТА С ОШИБКАМИ
===========================================
При клике на строке с ошибкой курсор в редакторе автоматически
перемещается к месту ошибки.";

            MessageBox.Show(helpText, "Справка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnAbout(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Текстовый редактор - Языковой процессор\n" +
                "Версия 2.0\n" +
                "Разработано в рамках курсовой работы\n" +
                "по дисциплине \"Теория формальных языков и компиляторов\"\n\n" +
                "© 2026",
                "О программе",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!ConfirmSaveChanges())
            {
                e.Cancel = true;
            }
            base.OnFormClosing(e);
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}