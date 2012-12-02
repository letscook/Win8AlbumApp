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

        // Define all image titles
        string[] ImageTitles = new string[] {
            "Title One",
            "Title Two",
            "Title Three",
            "Tap Image to Play",
            "Hello, World!",
            "Goodbye, World!",
        };

        // Define video strings paired with the image strings
        string[] ImageVideoFileNames = new string[] {
            "",
            "",
            "",
            "TestVideo.wmv",
            "",
            "",
        };

        // Current image index, and image count
        private int ImageIndex = 0;
        private int ImageCount = 0;

        // Rotation constant
        private double GRotates = 20.0;

        // Global random
        Random GRandom = new Random();

        // Video player in question
        MediaElement VideoPlayer = null;

        public MainPage()
        {
            this.InitializeComponent();

            // Kill interaction for images
            TitleLabel.IsHitTestVisible = false;
            TextLabel.IsHitTestVisible = false;

            // Image count set
            ImageCount = ImageFileNames.Length;

            // For each image
            for(int i = 0; i < ImageFileNames.Length; i++)
            {
                // Pull out image file name
                string ImageFileName = ImageFileNames[i];

                // Add all images to main rect...
                Image TestImage = new Image();
                TestImage.Source = new BitmapImage(new Uri(this.BaseUri, "/Assets/Album/" + ImageFileName));
                MainCanvas.Children.Add(TestImage);
                
                // Make sure images aren't interactable
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
            CompositeTransform Transform = new CompositeTransform();
            Transform.TranslateX = -TestImage.ActualWidth + 10;
            Transform.TranslateY = MainCanvas.ActualHeight / 2 - TestImage.ActualHeight / 2;
            Transform.Rotation = (GRandom.NextDouble() - 0.5) * GRotates;

            TestImage.RenderTransform = Transform;

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
        Point FirstPoint;

        /*** User Events ***/

        private void TestRectangle_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            FirstPoint = e.GetCurrentPoint(sender as UIElement).Position;
        }

        private void TestRectangle_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            Point LastPoint = e.GetCurrentPoint(sender as UIElement).Position;

            // Check for drag success
            CheckDrag(LastPoint);
        }

        private void CheckDrag(Point LastPoint)
        {
            // Ignore if there is a video playing...
            if (VideoPlayer != null)
                return;

            // Is the delta y minimal?
            int dx = (int)LastPoint.X - (int)FirstPoint.X;
            int dy = (int)LastPoint.Y - (int)FirstPoint.Y;
            double dRange = Math.Sqrt(dx * dx + dy * dy);

            // Click event
            if (ImageVideoFileNames[ImageIndex].Length > 0 && dRange < 25)
            {
                // Load up the video...
                VideoPlayer = new MediaElement();
                VideoPlayer.Source = new Uri(this.BaseUri, "/Assets/Album/" + ImageVideoFileNames[ImageIndex]);
                VideoPlayer.Play();
                VideoPlayer.Width = MainCanvas.ActualWidth;
                VideoPlayer.Height = MainCanvas.ActualHeight;

                // Add video ontop of main view
                MainCanvas.Children.Add(VideoPlayer);

                // Register button event in case user wants to cancel...
                VideoPlayer.PointerReleased += VideoPlayer_PointerReleased;
            }

            // Drag event
            else if (dy < 25 && dy > -25) // Horizontal drag
            {
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
        }

        private void VideoPlayer_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            // Sender is always video; kill video
            MediaElement Video = sender as MediaElement;
            Video.Stop();
            MainCanvas.Children.Remove(Video);
            VideoPlayer = null;
        }

        private void UpdateAnimation()
        {
            // Set the text
            TextLabel.Text = (ImageIndex + 1) + " / " + ImageCount;
            TitleLabel.Text = ImageTitles[ImageIndex];

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

                    DoubleAnimation RAnimation = new DoubleAnimation();
                    RAnimation.Duration = AnimationTime;

                    // Original x position of image
                    CompositeTransform Rotation = TargetImage.RenderTransform as CompositeTransform;
                    double OriginalX = Rotation.TranslateX;
                    double OriginalR = Rotation.Rotation;

                    // Randomize rotation to something reasonable
                    RAnimation.From = OriginalR;
                    RAnimation.To = OriginalR;

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
                        RAnimation.To = (GRandom.NextDouble() - 0.5) * GRotates; // [-0.5, 0.5] * range
                    }
                    else if(GroupIndex == 2)
                    {
                        Animation.From = OriginalX;
                        Animation.To = -TargetImage.ActualWidth + 10;
                    }

                    // Add x trans to storyboard
                    XAnimation.Children.Add(Animation);
                    XAnimation.Children.Add(RAnimation);
                    Storyboard.SetTarget(Animation, TargetImage);
                    Storyboard.SetTarget(RAnimation, TargetImage);
                    Storyboard.SetTargetProperty(Animation, "(UIElement.RenderTransform).(CompositeTransform.TranslateX)");
                    Storyboard.SetTargetProperty(RAnimation, "(UIElement.RenderTransform).(CompositeTransform.Rotation)");
                }

                // Done with commitment
                XAnimation.Begin();
            }
        }
    }
}
