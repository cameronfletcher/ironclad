# Samples #

All of the samples use HTTP as opposed to HTTPS when running.
This is for protocol debugging purposes only and should not be considered for a production environment.

### Sample Client
This sample demonstrates how to make a server-to-server call from a client application to a web API.  
*Note: This sample requires an instance of Ironclad and an instance of the Sample Web API to be running.*

### Sample Console Application
This sample demonstrates how to provide Single Sign-On functionality to a console application.  
*Note: This sample requires an instance of Ironclad and an instance of the Sample Web API to be running.*

### Sample Single Page Application (SPA)
This sample demonstrates how to provide Single Sign-On functionality to a Single Page Application (SPA) running with a browser. It further demonstrates how an SPA may make a secure call to a web API on behalf of the end-user. It uses the `oidc-client.js` library to handle all OpenID Connect based operations within the browser.  
*Note: This sample requires an instance of Ironclad and an instance of the Sample Web API to be running.*

### Sample Web API
This sample demonstrates how to secure a web API. It supports calls from both client credentials calls (server to server) or implicit calls (browser based) using an access token (either a JWT or a reference token). Once running this sample will listen on http://localhost:5007.  
*Note: This sample requires an instance of Ironclad to be running.*

#### Claims Transformation
The identity of the user or client may undergo transformation to augment or alter the claims with data specific to the service. A trivial example of this happening can be found in [ClaimsTransformation.cs](SampleWebApi/ClaimsTransformation.cs?fileviewer=file-view-default).

#### Cross Origin Resource Sharing
To verify that a call to the API is allowed the browser will typically issue a pre-flight OPTIONS request to the service. The response to this request can be configured in [Startup.cs](SampleWebApi/Startup.cs?fileviewer=file-view-default#Startup.cs-47).

#### Authorization Policies
Granular authorization policies can be defined which allow or restrict access based on configuration. These can be defined in [Startup.cs](SampleWebApi/Startup.cs?fileviewer=file-view-default#Startup.cs-19). To apply the policy to a specific call simply decorate the relevant method in the controller with `[Authorize("policy_name")]` where `policy_name` is the name of the configured policy.

