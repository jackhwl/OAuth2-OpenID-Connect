Adding the quickstart UI
iex ((New-Object System.Net.WebClient).DownloadString('https://raw.githubusercontent.com/IdentityServer/IdentityServer4.Quickstart.UI/master/getmaster.ps1'))

Response Type Values: 

| code | id_token / id_token token | code id_token / code token / code id_token token |
| -- | -- | -- |
| Authorization Code | Implicit | Hybird |

|Client application ||IDP|
| -- | -- | -- |
||authentication request --->|authorization endpoint|
|||user authenticates|
|||user gives consent|
||<----	code id_token (authrozation code, identity token) |
|token (identity token) is validated|
||token request (code, clientid, clientsecret) --->| 	token endpoint
||<----	id_token, access_token
token (identity token) is validated|
||userinfo request (access_token) ---> | userinfo endpoint
|||access token is validated
||<----	user claims

https://openid.net/specs/openid-connect-core-1_0.html

The OpenID Connect protocol, in abstract, follows the following steps.

The RP (Client) sends a request to the OpenID Provider (OP).
The OP authenticates the End-User and obtains authorization.
The OP responds with an ID Token and usually an Access Token.
The RP can send a request with the Access Token to the UserInfo Endpoint.
The UserInfo Endpoint returns Claims about the End-User.
These steps are illustrated in the following diagram:

+--------+                                   +--------+
|        |                                   |        |
|        |---------(1) AuthN Request-------->|        |
|        |                                   |        |
|        |  +--------+                       |        |
|        |  |        |                       |        |
|        |  |  End-  |<--(2) AuthN & AuthZ-->|        |
|        |  |  User  |                       |        |
|   RP   |  |        |                       |   OP   |
|        |  +--------+                       |        |
|        |                                   |        |
|        |<--------(3) AuthN Response--------|        |
|        |                                   |        |
|        |---------(4) UserInfo Request----->|        |
|        |                                   |        |
|        |<--------(5) UserInfo Response-----|        |
|        |                                   |        |
+--------+                                   +--------+


the main reason to get user claims this way is to keep identity token small.	
	

Marvin.IDP> info: IdentityServer4.Endpoints.AuthorizeEndpoint[0]
Marvin.IDP>       ValidatedAuthorizeRequest

Marvin.IDP> info: IdentityServer4.ResponseHandling.AuthorizeInteractionResponseGenerator[0]
Marvin.IDP>       User consented to scopes: openid, profile

Marvin.IDP> info: IdentityServer4.Endpoints.AuthorizeCallbackEndpoint[0]
Marvin.IDP>       Authorize endpoint response

ImageGallery.Client> info: Microsoft.AspNetCore.Hosting.Internal.WebHost[1]
ImageGallery.Client>       Request starting HTTP/1.1 POST http://localhost:44351/signin-oidc application/x-www-form-urlencoded 1524
Marvin.IDP> info: Microsoft.AspNetCore.Hosting.Internal.WebHost[1]
Marvin.IDP>       Request starting HTTP/1.1 POST http://localhost:44380/connect/token application/x-www-form-urlencoded 208

Marvin.IDP> info: IdentityServer4.Validation.TokenRequestValidator[0]
Marvin.IDP>       Token request validation success

Microsoft.AspNetCore.Hosting.Internal.WebHost:Information: Request starting HTTP/1.1 GET http://localhost:44380/connect/userinfo  
dentityServer4.Hosting.IdentityServerMiddleware:Information: Invoking IdentityServer endpoint: IdentityServer4.Endpoints.UserInfoEndpoint for /connect/userinfo

Configure(): UseXyzAuthentication has been replaced by ConfigureService(): AddXyz()
https://github.com/aspnet/Security/issues/1310
ClientSecret is required in Hybird mode 

By default, identity server will not include the claims in the identity token.
there're two ways to include claims 
1. AlwaysIncludeUserClaimsInIdToken = true (IDP Config.cs)  ---> not recommanded
2. Calling the UserInfo Endpoint to Get Additional Claims

multiple project:
ImageGallery.API		start
ImageGallery.Client		start
ImageGallery.Model		none
Marvin.IDP				none

https://github.com/aspnet/Security/blob/dde7671c06da64e4a7a290c37ed86e9a9bdd0dd7/src/Microsoft.AspNetCore.Authentication.OpenIdConnect/OpenIdConnectOptions.cs#L63-L69
// http://openid.net/specs/openid-connect-core-1_0.html#StandardClaims
            ClaimActions.MapUniqueJsonKey("sub", "sub");
            ClaimActions.MapUniqueJsonKey("name", "name");
            ClaimActions.MapUniqueJsonKey("given_name", "given_name");
            ClaimActions.MapUniqueJsonKey("family_name", "family_name");
            ClaimActions.MapUniqueJsonKey("profile", "profile");
            ClaimActions.MapUniqueJsonKey("email", "email");

Claims not on above list will not be include in id-token by default.

call discoveryClient to get metaDataResponse, then use userinfoEndpoint from metaDataResponse

RBAC (Role-based access control) vs ABAC (Attribute-based access control)

need add role specifically in startup class
options.ClaimActions.MapUniqueJsonKey("role", "role");

