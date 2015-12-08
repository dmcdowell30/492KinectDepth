%Change the file name based on the what output file you are testing
M = csvread( 'C:\KinectData\KinectScreenshot-Depth-06-38-37-Output.csv' )
N=1000-M
O=N
O(O>400)=0
O(O<-875)=0
P=O(200:250,1:end)
P(P<100)=100

slice=P(1:15,1:end)

line=mean(slice)

diffs = mean(line) - line
plot(diffs)

xaxis=(0:size(P,2)-1)

%f is for the x direction, f4 is for the y direction
%plot(diffs); hold; plot(f4); hold;
%coeffs=[f4.p1 f4.p2 f4.p3 f4.p4 f4.p5]
plot(diffs); hold; plot(f); hold;
coeffs=[f.p1 f.p2 f.p3 f.p4 f.p5 f.p6]

x=0:size(P,2)-1

polyvector=polyval(coeffs,x)

correction=repmat(polyvector,51,1)

altered=P+correction

surf(P, 'FaceColor', 'interp', 'EdgeColor', 'none', 'FaceLighting', 'gouraud')
camlight left

%find the root mean square error for the output
e=mean(rms(altered-P))
d = (e + 3.2930) / 2
