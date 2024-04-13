using System;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json.Linq;

internal class Program
{
    static string AppKey = "02619EF1A99F54F199590E871ED8B9C2";
    static string AccessToken;
    static string GlobalTaskId;
    private static async Task Main(string[] args)
    {
        Console.WriteLine("=======================================");
        Console.WriteLine("欢迎来到天蛙云全自动抢课!");
        Console.WriteLine("Developed by Aunt_nuozhen@Aunt Studio");
        Console.WriteLine("Source code are open under GNU GENERAL PUBLIC LICENSE V3");
        Console.WriteLine("Github: https://github.com/yangnuozhen/selectCourse");
        Console.WriteLine("请对您自己的行为负责任。");
        Console.WriteLine("=======================================\n");
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
        Console.WriteLine("正在获取选课任务列表...");
        await selectTargetTask();
        Console.WriteLine("正在获取选课列表...");
        await selectTargetCourse();
        Console.WriteLine("请输入欲报名的课程对应的ClassID。");
        Console.WriteLine("ClassID可由上方输出的课程列表复制对应选课的classId取得。");
        string classIds = Console.ReadLine();
        Console.WriteLine("请输入尝试间隔(单位: 毫秒, 只输入整数)。建议不要太快(5000以上)，否则可能会被服务器Nginx 429 Too Many Requests");
        int delayTime = int.Parse(Console.ReadLine());
        Console.WriteLine("===============开始尝试===============");
        int times = 1;
        while(true)
        {
            try
            {
                string back = await PostRequest($"https://gateway.tianwayun.com/apps/course/stu/selectTask/selectTimeCourse?taskId={GlobalTaskId}&classIds={classIds}", AccessToken, AppKey);
                
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
                Console.WriteLine("\nError: 登录失败。请检查用户名与密码。服务器Response:\n");
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
    private static async Task selectTargetCourse()
    {
        string url = $"https://gateway.tianwayun.com/apps/course/stu/selectTask/getByTimeTaskId?taskId={GlobalTaskId}";
        string r;

        Console.Write("正在发送请求包...");
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("AccessToken", AccessToken);
            client.DefaultRequestHeaders.Add("AppKey", AppKey);
            HttpResponseMessage response = await client.GetAsync(url);
            r = await response.Content.ReadAsStringAsync();
            
        }
        Console.WriteLine("OK");
        Console.WriteLine("开始解析并输出选课列表\n");
        

        JObject json = JObject.Parse(r);
        if (json["code"]?.ToString() != "1")
        {
            Console.WriteLine("\nError: 无法获得选课列表。服务器Response:\n");
            Console.WriteLine(r);
            System.Environment.Exit(1);

        }
        Console.WriteLine("==============================================");
        int UserListCount = json["data"]["courseDTOS"][0]["classDTOS"].Count();
        for (int i = 0; i < UserListCount; i++)
        {
            
            Console.WriteLine($"[{i+1}]\n | 课程名称:{json["data"]["courseDTOS"][0]["classDTOS"][i]["className"]}" );
            Console.WriteLine($" | 授课教师:{json["data"]["courseDTOS"][0]["classDTOS"][i]["teacherName"]}");
            Console.WriteLine($" | ClassID:{json["data"]["courseDTOS"][0]["classDTOS"][i]["classId"]}");
            Console.WriteLine();
            
        }
        Console.WriteLine("==============================================");
    }
    private static async Task selectTargetTask()
    {
        string url = "https://gateway.tianwayun.com/apps/course/stu/selectTask/list?endFlag=false";
        string r;
        List<string> taskName = new List<string>();
        List<string> semesterName = new List<string>();
        List<string> beginTime = new List<string>();
        List<string> endTime = new List<string>();
        List<string> completeStat = new List<string>();
        List<string> taskId = new List<string>();
        Console.Write("正在发送请求包...");
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("AccessToken", AccessToken);
            client.DefaultRequestHeaders.Add("AppKey", AppKey);
            HttpResponseMessage response = await client.GetAsync(url);
            r = await response.Content.ReadAsStringAsync();

        }
        Console.WriteLine("OK");
        Console.WriteLine("开始解析并输出选课任务列表\n");
        JObject json = JObject.Parse(r);
        if (json["code"]?.ToString() != "1")
        {
            Console.WriteLine("\nError: 无法获得选课任务列表。\nCoursed By: 服务器回应code非1.\n服务器Response:\n");
            Console.WriteLine(r);
            System.Environment.Exit(1);

        }
        Console.WriteLine("==============================================");
        var rows = json["data"]["rows"];
        for (int i = 0; i < rows.Count(); i++)
        {
            taskName.Add(rows[i]["taskName"].ToString());
            semesterName.Add(rows[i]["semesterName"].ToString());
            beginTime.Add(rows[i]["beginTimeStr"].ToString());
            endTime.Add(rows[i]["endTimeStr"].ToString());
            completeStat.Add(rows[i]["completeStatusZh"].ToString());
            taskId.Add(rows[i]["taskId"].ToString());

            Console.WriteLine($"[{i + 1}] | 选课任务名称:{taskName[i]}");
            Console.WriteLine($"        | 学期:{semesterName[i]}");
            Console.WriteLine($"        | 状态:{completeStat[i]}");
            Console.WriteLine($"        | 选课时间:{beginTime[i]} - {endTime[i]}");
            Console.WriteLine($"        | TaskID:{taskId[i]}");
            Console.WriteLine("\n------------------------------------------------\n");

        }
        Console.WriteLine("==============================================\n");
        Console.WriteLine("请输入选课任务前的序号: ");
        int select = int.Parse(Console.ReadLine());
        GlobalTaskId = taskId[select - 1];

        Console.WriteLine($"成功, 当前选课任务为: {taskName[select - 1]}。");
    }
}