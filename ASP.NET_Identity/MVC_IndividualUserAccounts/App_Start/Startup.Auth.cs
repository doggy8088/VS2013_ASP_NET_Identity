using Owin;

namespace MVC_IndividualUserAccounts
{
    public partial class Startup
    {
        // 如需設定驗證的詳細資訊，請瀏覽 http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // 讓應用程式使用 Cookie 儲存已登入使用者的資訊
            // 以及使用 Cookie 暫時儲存與使用第三方登入提供者登入的使用者相關的資訊
            app.UseSignInCookies();

            // 註銷下列各行以啟用利用協力廠商登入提供者登入
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //   consumerKey: "",
            //   consumerSecret: "");

            //app.UseFacebookAuthentication(
            //   appId: "",
            //   appSecret: "");

            //app.UseGoogleAuthentication();
        }
    }
}