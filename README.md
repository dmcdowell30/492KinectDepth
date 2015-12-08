# 492KinectDepth
Getting Kinect depth data for soil microtopography

This README should help setup our projects to run on a laptop.

For this setup the user will need to download the Windows Kinect SDK v2 onto a laptop or desktop computer running Windows 8 or later and have a folder named C# located at C:\KinectData\. 
This will install the necessary libraries and resources that our program will need to run and allow the program to save to the correct location. 

Then when that is done they will need to have the DepthBasics-WPF.exe file that has been created by our program from Visual Studio or they can download our source code and open the project in Visual Studio. 
There is a Github link on our team website for our source code. 

Run the program DepthBasics-WPF.exe. 
If the SDK is installed correctly, then this executable should display a window that has another display window in the center of it as well as a button in the bottom right corner.
The inner display will show as black in the Kinect is not already connected to a USB 3.0 slot on the computer, otherwise, it will display what the Kinect sensors are seeing. 
This will help the user see what the data will show and will allow them to see the deadzones. 

When the user is happy with the data, they can press the button to capture the data and save the information to a location on their computer. 
The window will tell the user where the files are saved to.
 
