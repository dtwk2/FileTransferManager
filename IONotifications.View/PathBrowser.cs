using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Prism.Commands;
using Button = System.Windows.Controls.Button;
using Control = System.Windows.Controls.Control;
using Label = System.Windows.Controls.Label;
using TextBox = System.Windows.Controls.TextBox;

namespace IOExtensions.View
{


    /// <summary>
    /// Interaction logic for PathBrowser.xaml
    /// </summary>
    public abstract class PathBrowser : Control
    {
        protected TextBox TextBoxOne;
        protected Subject<Unit> applyTemplateSubject = new Subject<Unit>();
        protected Button ButtonOne;
        protected ContentControl ContentControlOne;
        protected Label LabelOne;
        protected Subject<TextBox> textBoxSubject = new Subject<TextBox>();
        protected Subject<string> textChanges = new Subject<string>();

        static PathBrowser()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PathBrowser), new FrameworkPropertyMetadata(typeof(PathBrowser)));
        }

        public PathBrowser()
        {
            applyTemplateSubject
                .CombineLatest(textBoxContentChanges, (a, b) => b)
                .Subscribe(a =>
                {
                    this.ContentControlOne.Content = a;
                    (a as DependencyObject)
                        .GetChildrenOfType<TextBox>()
                        .ToObservable()
                        .Merge(Observable.Return(a as TextBox).Where(b => b != null))
                        .Subscribe(textBoxSubject.OnNext);
                });


            textBoxSubject
                .SelectMany(ObservableHelper.ToThrottledObservable)
                .Subscribe(textChanges.OnNext);

            SetPath = new DelegateCommand<string>(textChanges.OnNext);

            textChanges
                .DistinctUntilChanged()
                .CombineLatest(textBoxSubject.StartWith(default(TextBox)), (a, b) => (a, b))
                .SubscribeOnDispatcher()
                .Subscribe(a =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var (path, textBox) = a;
                        OnTextChange(path, textBox);
                    });
                });
        }


        public override void OnApplyTemplate()
        {

            this.ButtonOne = this.GetTemplateChild("ButtonOne") as Button;
            this.ContentControlOne = this.GetTemplateChild("ContentControlOne") as ContentControl;
            TextBoxOne = (this.GetTemplateChild("StackPanelOne") as FrameworkElement).Resources["TextBoxOne"] as TextBox;
            this.TextBoxContent = this.TextBoxContent ??= TextBoxOne;
            this.LabelOne = this.GetTemplateChild("LabelOne") as Label;
            LabelOne.Content = Label;
            base.OnApplyTemplate();
            applyTemplateSubject.OnNext(Unit.Default);
        }

        protected virtual void OnTextChange(string path, TextBox sender)
        {
            var textBox = TextBoxContent as TextBox;
            textBox.Text = path;
            textBox.Focus();
            var length = System.IO.Path.GetFileName(path).Length;
            textBox.Select(path.Length - length, length);
            textBox.ToolTip = path;
            RaiseTextChangeEvent(path);
        }


        protected abstract (bool? result, string path) OpenDialog(string filter, string extension);


        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(PathBrowser), new PropertyMetadata(null));


        public object TextBoxContent
        {
            get { return (object)GetValue(TextBoxContentProperty); }
            set { SetValue(TextBoxContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextBoxContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextBoxContentProperty =
            DependencyProperty.Register("TextBoxContent", typeof(object), typeof(PathBrowser), new PropertyMetadata(null, TextBoxContentChanged));

        private Subject<object> textBoxContentChanges = new Subject<object>();

        private static void TextBoxContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as PathBrowser).textBoxContentChanges.OnNext(e.NewValue);
        }




        public ICommand SetPath
        {
            get { return (ICommand)GetValue(SetPathProperty); }
            set { SetValue(SetPathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SetPath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SetPathProperty =
            DependencyProperty.Register("SetPath", typeof(ICommand), typeof(PathBrowser), new PropertyMetadata(null));




        public static readonly RoutedEvent TextChangeEvent = EventManager.RegisterRoutedEvent("TextChange", RoutingStrategy.Bubble, typeof(TextChangeRoutedEventHandler), typeof(PathBrowser));

        // Provide CLR accessors for the event
        public event TextChangeRoutedEventHandler TextChange
        {
            add => AddHandler(TextChangeEvent, value);
            remove => RemoveHandler(TextChangeEvent, value);
        }

        protected void RaiseTextChangeEvent(string text)
        {
            this.Dispatcher.Invoke(() => RaiseEvent(new TextRoutedEventArgs(TextChangeEvent, text)));
        }

        public class TextRoutedEventArgs : RoutedEventArgs
        {
            public TextRoutedEventArgs(RoutedEvent routedEvent, string text) : base(routedEvent)
            {
                this.Text = text;
            }

            public string Text { get; set; }
        }

        public delegate void TextChangeRoutedEventHandler(object sender, TextRoutedEventArgs e);

    }
}
