# uncmd
Its command line tool for the utility network.  It has options to validate topology, update all subnetworks and update is connected for a given service or all services for the logged in user. Examples of how this works:

Validate full extent for a given service AllStar_Postgres

> **uncmd.exe** /url:utilitynetwork.esri.com/portal /user:unadmin /pass:unadmin /s:AllStar_Postgres /v

Validate full extent, update subnetworks and update is connected for ALL services.

> **uncmd.exe** /url:utilitynetwork.esri.com/portal /user:unadmin /pass:unadmin /s:all /v /u /c

Validate specific extent

> **uncmd.exe** /url:utilitynetwork.esri.com/portal /user:unadmin /pass:unadmin /s:NapervilleElectric_Oracle_Daily /v /e:{"xmin":1034298.770580286,"ymin":1871150.45390869863,"xmax":1035343.74091765482,"ymax":1871932.05773827527,"spatialReference":{"wkid":102671,"latestWkid":3435}}


Enable logging at any time by adding /l this will write to a folder "logs" under the same directory where uncmd.exe lives

> **uncmd.exe** /url:utilitynetwork.esri.com/portal /user:unadmin /pass:unadmin /s:AllStar_Postgres /v /l

Download tool here  https://github.com/hussein-nasser/uncmd/releases
