# 492KinectDepth
Getting Kinect depth data for soil microtopography

This README should help setup our projects to run on a laptop.

=============== FOR THE C# PROGRAM ===============
For this setup the user will need to download the Windows Kinect SDK v2 onto a laptop or desktop computer running Windows 8 or later and have a folder named C# located at C:\KinectData\. 
This will install the necessary libraries and resources that our program will need to run and allow the program to save to the correct location. 

Then when that is done they will need to have the DepthBasics-WPF.exe file (located in bin/AnyCPU/Debug) that has been created by our program from Visual Studio or they can download our source code and open the project in Visual Studio. 
There is a Github link on our team website for our source code. 

Run the program DepthBasics-WPF.exe. 
If the SDK is installed correctly, then this executable should display a window that has another display window in the center of it as well as a button in the bottom right corner.
The inner display will show as black in the Kinect is not already connected to a USB 3.0 slot on the computer, otherwise, it will display what the Kinect sensors are seeing. 
This will help the user see what the data will show and will allow them to see the deadzones. 

When the user is happy with the data, they can press the button to capture the data and save the information to a location on their computer. 
The window will tell the user where the files are saved to.

=============== FOR MATLAB SCRIPT ===============
Included in this repo is the script that we ran to find our curve polynomials.

To open the CalibrationTests.m file, you first need the MatLab application.

Once you get that simply double click on the file and it should open.

To change data sets, simply change the file source at the top of the script to where you are keeping your CSV files.

To get correct data, run the script once. 
It should say an error was found dealing with either f or f4 not being defined.

This is where you open the curve fitting tool and plot the data.
The X values should be the variable xaxis and the Y values should be from diffs.

A graph should appear and this is where you can change the polynomial degree.

Get a close fit and then save the polynomial to the workspace as either f or f4.

Then go back to the script and rerun the program and it should display your data. 

NOTE: if the data is already corrected in the C# program, you won't see much of a change here.


=============== FOR C PROGRAM ===============
To use CProg.c, you will first need to compile it into an program using "gcc -o programName CProg.c" on a Linux device.

Then you should be able to run it by doing ./programName.
Once started it should appear blank on the command line.

Enter a number and hit ENTER and it should create a child process that will read a text file and display it to console.
The default name it looks for is "test.txt"
