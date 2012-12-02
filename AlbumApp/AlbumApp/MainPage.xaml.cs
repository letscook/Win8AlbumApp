using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Popups;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Media.Animation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace AlbumApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // Define all image names (not the correct dir)
        string[] ImageFileNames = new string[] {
            "Landscape1.png",
            "Landscape2.png",
            "Landscape3.png",
            "PlayVideo.png",
            "Portrait1.png",
            "Portrait2.png",
        };

        // Current image index, and image count
        private int ImageIndex = 0;
        private int ImageCount = 0;

        public MainPage()
        {
            this.InitializeComponent();

            // Image count set
            ImageCount = ImageFileNames.Length;

            // For each image
            foreach (string ImageFileName in ImageFileNames)
            {
                // Add all images to main rect...
                Image TestImage = new Image();
                TestImage.Source = new BitmapImage(new Uri(this.BaseUri, "/Assets/Album/" + ImageFileName));
                MainCanvas.Children.Add(TestImage);
                TestImage.IsHitTestVisible = false;

                // Once an image is loaded; move off to side
                TestImage.ImageOpened += TestImage_Loaded;
            }
        }

        // Total loaded
        int ImagesLoaded = 0;
        
        void TestImage_Loaded(object sender, RoutedEventArgs e)
        {
            // Move image to the side and invisible
            Image TestImage = sender as Image;
            TranslateTransform SideTrans = new TranslateTransform();
            SideTrans.X = -TestImage.ActualWidth + 10;
            SideTrans.Y = MainCanvas.ActualHeight / 2 - TestImage.ActualHeight / 2;
            TestImage.RenderTransform = SideTrans;

            ImagesLoaded++;

            if (ImagesLoaded >= ImageCount)
                UpdateAnimation();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
        }

        // Register starting point (null if no interaction)
        bool IsDragging = false;
        Point FirstPoint;

        /*** User Events ***/

        private void TestRectangle_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            IsDragging = true;
            FirstPoint = e.GetCurrentPoint(sender as UIElement).Position;
        }

        private void TestRectangle_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IsDragging = false;
            Point LastPoint = e.GetCurrentPoint(sender as UIElement).Position;

            // Check for drag success
            CheckDrag(LastPoint);
        }

        private void TestRectangle_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            IsDragging = false;
            Point LastPoint = e.GetCurrentPoint(sender as UIElement).Position;

            // Check for drag success
            //CheckDrag(LastPoint);
        }

        private void CheckDrag(Point LastPoint)
        {
            // Is the delta y minimal?
            int dx = (int)LastPoint.X - (int)FirstPoint.X;
            int dy = (int)LastPoint.Y - (int)FirstPoint.Y;

            // If the pixel delta is small, just ignore
            if (dy > 100)
                return;

            // Else, we figure out direction
            if (dx < -100 && ImageIndex > 0)
            {
                // Move to left image
                ImageIndex--;
                UpdateAnimation();
            }
            else if (dx > 100 && ImageIndex < ImageCount - 1)
            {
                // Move to right image
                ImageIndex++;
                UpdateAnimation();
            }
        }

        private void UpdateAnimation()
        {
            // Set the text
            TextLabel.Text = (ImageIndex + 1) + " / " + ImageCount;

            /*** Move all to the left ***/

            // For each group type (left, visible, right)
            for (int GroupIndex = 0; GroupIndex < 3; GroupIndex++)
            {
                // How long the animations are
                Duration AnimationTime = new Duration(TimeSpan.FromSeconds(0.5));

                // Animate all the pics!
                Storyboard XAnimation = new Storyboard();
                XAnimation.Duration = AnimationTime;

                // Based on the group, define the start or end indices
                int StartIndex = 0;
                int EndIndex = ImageCount;

                if (GroupIndex == 0)
                {
                    StartIndex = 0;
                    EndIndex = ImageIndex;
                }
                else if (GroupIndex == 1)
                {
                    StartIndex = ImageIndex;
                    EndIndex = ImageIndex + 1;
                }
                else if(GroupIndex == 2)
                {
                    StartIndex = ImageIndex + 1;
                    EndIndex = ImageCount;
                }

                // For each previous element, move to left screen
                for (int i = StartIndex; i < EndIndex; i++)
                {
                    // Image in question
                    Image TargetImage = MainCanvas.Children[i] as Image;

                    // Define animation
                    DoubleAnimation Animation = new DoubleAnimation();
                    Animation.Duration = AnimationTime;

                    // Original x position of image
                    // NOTE: this is the official approach; how insaine is this?!
                    double OriginalX = TargetImage.TransformToVisual(MainCanvas).TransformPoint(new Point()).X;

                    // Stay left
                    if (GroupIndex == 0)
                    {
                        Animation.From = OriginalX;
                        Animation.To = MainCanvas.ActualWidth - 10;
                    }
                    // Go middle
                    else if (GroupIndex == 1)
                    {
                        Animation.From = OriginalX;
                        Animation.To = MainCanvas.ActualWidth / 2 - TargetImage.ActualWidth / 2;
                    }
                    else if(GroupIndex == 2)
                    {
                        Animation.From = OriginalX;
                        Animation.To = -TargetImage.ActualWidth + 10;
                    }
                    
                    // Add x trans to storyboard
                    XAnimation.Children.Add(Animation);
                    Storyboard.SetTarget(Animation, TargetImage);
                    Storyboard.SetTargetProperty(Animation, "(UIElement.RenderTransform).(TranslateTransform.X)");
                }

                // Done with commitment
                XAnimation.Begin();
            }
        }
    }
}
