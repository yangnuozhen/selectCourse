using System;
using System.Text;
using Newtonsoft.Json.Linq;

internal class Program
{
    static string AppKey = "02619EF1A99F54F199590E871ED8B9C2";
    static string AccessToken;
    private static async Task Main(string[] args)
    {
        
        Console.WriteLine("请输入用户名。");
        Console.WriteLine("通常情况下，用户名是你的学籍号，也就是G+身份证号。");
        Console.WriteLine();
        string AccountName = Console.ReadLine();
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("请输入密码。");
        Console.WriteLine("在你没有手动修改密码的情况下，密码默认为你的学籍号后6位。");
        Console.WriteLine();
        string Password = Console.ReadLine();
        Console.WriteLine();
        Console.WriteLine("==============Please Wait==============");

        AccessToken = await Login(AccountName, Password); 

        Console.WriteLine("===============登录结束===============");
        Console.WriteLine();
        Console.WriteLine("请输入欲报名的课程对应的ClassID。");
        Console.WriteLine("ClassID可由getByTimeTaskId 返回值中对应选课的classId取得。");
        Console.WriteLine("什么？你不会抓包？请寻找Aunt_nuozhen获得ClassID。");
        string classIds = Console.ReadLine();
        //修改classIds来修改选择的课程。
        //classIds可由getByTimeTaskId 取得
        //什么？你不会抓包？请寻找Aunt_nuozhen获得ClassID。
        Console.WriteLine("请输入尝试间隔(单位: 毫秒, 只输入整数)。建议不要太快(5000以上)，否则可能会被服务器Nginx 429 Too Many Requests");
        int delayTime = int.Parse(Console.ReadLine());
        Console.WriteLine("===============开始尝试===============");
        int times = 1;
        while(true)
        {
            try
            {
                string back = await PostRequest($"https://gateway.tianwayun.com/apps/course/stu/selectTask/selectTimeCourse?taskId=281C2B5B3DBE6CDE514134A3AC501952&classIds={classIds}", AccessToken, AppKey);
                
                Console.WriteLine($"The {times}st try.");
                Console.WriteLine(back);
                Console.WriteLine("====================================");
                await Task.Delay(delayTime);
                times++;
            }catch (Exception ex)
            {
                Console.WriteLine("============Fatal Error!============");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("============Fatal Error!============");
                break;
            }

        }
        Console.WriteLine("Good Night.");
    }

    private static async Task<string> PostRequest(string url, string AccessToken, string AppKey)
    {
        using (HttpClient client = new HttpClient())
        {
            StringContent content = new StringContent("", Encoding.UTF8);
            client.DefaultRequestHeaders.Add("AccessToken", AccessToken);
            client.DefaultRequestHeaders.Add("AppKey", AppKey);
            HttpResponseMessage response = await client.PostAsync(url, content);
            string responseContent = await response.Content.ReadAsStringAsync();
            return responseContent;
        }
    }
    private static async Task<string> Login(string accountName, string password)
    {
        try
        {
            Console.Write("正在发送登录包...");
            string r;
            using (HttpClient client = new HttpClient())
            {
           
                string url = "https://sso.tianwayun.com/login?flag=login";
                StringContent content = new StringContent($"sourceType=TW_CLOUD_SSO&userName={accountName}&userPwd={password}&verifyToken=&verifyCode=", Encoding.UTF8, "application/x-www-form-urlencoded");
                client.DefaultRequestHeaders.Add("AppKey", AppKey);
                HttpResponseMessage response = await client.PostAsync(url, content);
                r = await response.Content.ReadAsStringAsync();
            
            }
            Console.WriteLine("OK");
            Console.Write("正在解析AccessToken...");
            JObject json = JObject.Parse(r);
            if (json["code"]?.ToString() != "1")
            {
                Console.WriteLine("\nError: 登录失败。服务器请检查用户名与密码。服务器Response:\n");
                Console.WriteLine(r);
                System.Environment.Exit(1);

            }
            string AccessToken = json["data"]["accessToken"].ToString();
            Console.WriteLine("OK");
            return AccessToken;
        }
        catch (Exception ex)
        {
            Console.WriteLine("============Fatal Error While Login!============");
            Console.WriteLine(ex.ToString());
            Console.WriteLine("============Fatal Error While Login!============");
            return "";
        }
        

    }
}