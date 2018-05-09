# DICOM_viewer
A Unity hololens viewer for DICOM images


## Setup

1. Create a zip file containing your DICOM media directories  
2. Deploy the application to the device once  
3. Connect the device via usb, and navigate to http://localhost:10080/FileExplorer.htm - USB will give better/more reliable data transfer  
4. Navigate to LocalAppData\DICOM_viewer\LocalState  
5. Click the Choose File button, and select your zip file. Do not click Upload - at the time of writing, there is a bug in the file explorer (see https://stackoverflow.com/questions/46409655/transfer-file-to-hololens-via-post-httprequest-error-429/47086736#47086736) - instead, enter this code in the javascript console (F12) - 
```javascript
(function(jQuery) {
var pathLinkData = jQuery(".pathLink:last-child").data(),
    path = pathLinkData.path,
    packagename = pathLinkData.packagename,
    knownfolderid = pathLinkData.knownfolderid,
    url = '/api/filesystem/apps/file?knownfolderid=' + knownfolderid + '&packagefullname=' + packagename + '&path=%5C%5C' + path,
    file_data = jQuery('#fileToUpload')[0].files[0],
    form_data = new FormData();

  form_data.append('file', file_data, file_data.name);

  jQuery.ajax({
    url: url,
    dataType: 'text',
    cache: false,
    contentType: false,
    processData: false,
    data: form_data,
    type: 'POST',
    error: function(xhr, textStatus, error) { console.error(error) },

  xhr: function() {
    var xhr = new window.XMLHttpRequest();

    xhr.upload.addEventListener("progress", function(evt) {
      if (evt.lengthComputable) {
        var percentComplete = evt.loaded / evt.total;
        percentComplete = parseInt(percentComplete * 100);
        console.log(percentComplete + "% done");
      }
    }, false);

    return xhr;
  },
    success: function(res){
      alert('uploaded');
      console.log(res);
    }
  });
})(jQuery);
```
 - this is mostly based on doublerebel's stack overflow snippet, I just added a progress indicator  
6. Run the application and it will unzip your zip file  

## Troubleshooting

If you have issues building to the device (dll version conflicts) - in the built VS project, open Tools->NuGet Package Manager->Manage NuGet Packages For Solution and update Microsoft.NETCore.UniversalWindowsPlatform