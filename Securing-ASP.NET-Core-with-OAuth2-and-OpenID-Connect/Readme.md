Adding the quickstart UI
iex ((New-Object System.Net.WebClient).DownloadString('https://raw.githubusercontent.com/IdentityServer/IdentityServer4.Quickstart.UI/master/getmaster.ps1'))

Response Type Values: 

code					id_toke	/ id_token token		code id_token / code token / code id_token token
Authorization Code		Implicit						Hybird

Configure(): UseXyzAuthentication has been replaced by ConfigureService(): AddXyz()
https://github.com/aspnet/Security/issues/1310
ClientSecret is required in Hybird mode 

multiple project:
ImageGallery.API		start
ImageGallery.Client		start
ImageGallery.Model		none
Marvin.IDP				none