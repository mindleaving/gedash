# Network communication with GE Dash 4000 patient monitor
The GE Dash 4000 is a patient monitor for monitoring vital signs, such as ECG, SpO2 (oxygen saturation of blood), respiration and a few other parameters.
See my website for details: https://www.janscholtyssek.dk/blog-all/project-ge-dash-4000-how-to-decode-a-network-protocol/

The GE Dash 4000 is usually part of a network of monitors and monitoring machines, called CARESCAPE Network (formerly Unite Network, as far as I know) (http://www3.gehealthcare.com/en/products/categories/patient_monitoring/networking/carescape_network) using a custom network protocol. This project was about decoding enough of the protocol for retrieving waveforms and parameters, such that I can store them continuously. The purpose is to use the monitor for sleep monitoring.
