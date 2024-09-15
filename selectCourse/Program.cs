using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json.Linq;

internal class Program
{
    private static readonly string AppKey = "02619EF1A99F54F199590E871ED8B9C2";
    static string AccessToken;
    static string GlobalTaskId;
    static string GlobalClassId;
    static DateTime GlobalStartTime;
    static int mode = 0; //0 = 循环尝试 1 = 定时尝试
    private static async Task Main(string[] args)
    {
        // 获取当前程序集
        Assembly assembly = Assembly.GetExecutingAssembly();
        Version version = assembly.GetName().Version;
        Console.WriteLine("""

              _________          _______                           _____      _           _   
             |__   __\ \        / / ____|                         / ____|    | |         | |  
                | |   \ \  /\  / / |     ___  _   _ _ __ ___  ___| (___   ___| | ___  ___| |_ 
                | |    \ \/  \/ /| |    / _ \| | | | '__/ __|/ _ \\___ \ / _ \ |/ _ \/ __| __|
                | |     \  /\  / | |___| (_) | |_| | |  \__ \  __/____) |  __/ |  __/ (__| |_ 
                |_|      \/  \/   \_____\___/ \__,_|_|  |___/\___|_____/ \___|_|\___|\___|\__|
                                                                                              
                                                                                              

            """);
        Console.WriteLine("=======================================");
        Console.WriteLine("欢迎来到天蛙云全自动抢课!");
        Console.WriteLine("Developed by Aunt_nuozhen @ Aunt Studio");
        Console.WriteLine("Source code are open under GNU GENERAL PUBLIC LICENSE V3");
        Console.WriteLine("Github: https://github.com/yangnuozhen/selectCourse");
        Console.WriteLine($"程序版本号: {version}");
        Console.WriteLine("请对您自己的行为负责任。");
        Console.WriteLine("=======================================\n");
        Console.WriteLine("正在检查更新 (方法: GitHub API)...");
        string ghApiResponse = await GetRequest("https://api.github.com/repos/yangnuozhen/selectCourse/releases/latest");
        JObject json = JObject.Parse(ghApiResponse);
        if (json["name"].ToString() != version.ToString())
        {
            Console.WriteLine("检测到可能可以利用的更新:");
            Console.WriteLine($"""
                =====================================
                来源: GitHub Releases API
                发行版名称: {json["name"]}
                发布时间: {ConvertUtcToBeijingTime(json["published_at"].ToString())}
                ID: {json["id"]}
                下载链接: {json["html_url"]}
                =====================================
                建议您始终使用最新版本。


                """);
        }
    INPUT_USERNAME: Console.WriteLine("请输入用户名。");
        Console.WriteLine("通常情况下，用户名是你的学籍号，也就是G+身份证号。");
        Console.WriteLine();
        string AccountName = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(AccountName))
        {
            Console.WriteLine("用户名不能为空.");
            goto INPUT_USERNAME;
        }
        Console.WriteLine();
        Console.WriteLine();
    INPUT_PASSWORD: Console.WriteLine("请输入密码。");
        Console.WriteLine("在你没有手动修改密码的情况下，密码默认为你的学籍号后6位。");
        Console.WriteLine();
        string Password = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(Password))
        {
            Console.WriteLine("密码不能为空.");
            goto INPUT_PASSWORD;
        }
        Console.WriteLine();
        Console.WriteLine("==============Please Wait==============");

        AccessToken = await Login(AccountName, Password);

        Console.WriteLine("===============登录结束===============");
        Console.WriteLine();
        Console.WriteLine("正在获取选课任务列表...");
        await InquireSelectTargetTask();
        Console.WriteLine("正在获取选课列表...");
        await InquireSelectTargetCourse();
    INPUT_MODE: Console.WriteLine("请选择尝试模式。\n[0]: 循环尝试\n[1]: 定时尝试");
        Console.WriteLine("""
            
            说明: 
            [0]: 循环尝试，即在程序开始运行后将立即开始不断重复发送数据包以尝试提交选课。
                 对于某一些特殊情况(例如，后台突然开放提交) 下较为保险，但不断发送数据包有一定概率会导致服务器封禁你的IP。

            [1]: 定时尝试，即在程序开始运行后将先获取选课任务的开放提交时间，并直到开放选课前几秒才开始尝试发送数据包。
                 该方法存在小概率会导致选课没有及时被提交，例如系统计时器出现错误、突然开放提交等。
                 使用此模式，请务必确保您的计算机系统时间精确，否则可能会导致提交的延迟。
                 在开始尝试选课前，会自动重新登录以刷新令牌。
            
            建议 (默认) 值: 1

            """);
        Console.WriteLine("请选择尝试模式: ");
        if (!int.TryParse(Console.ReadLine(), out mode) || mode < 0 || mode > 1)
        {
            Console.WriteLine("非法的输入。");
            goto INPUT_MODE;
        }
        switch (mode)
        {
            case 0:
            INPUT_DELAYTIME: Console.WriteLine("请输入尝试间隔(单位: 毫秒, 只输入整数)。建议不要太快(800以上)，否则可能会被服务器Nginx 429 Too Many Requests");
                int delayTime;
                if (!int.TryParse(Console.ReadLine(), out delayTime))
                {
                    Console.WriteLine("无法将您的输入转换为整数。");
                    goto INPUT_DELAYTIME;
                }
                Console.WriteLine("===============开始尝试 请勿关闭命令行窗口===============");
                int times = 1;
                while (true)
                {
                    try
                    {
                        string back = await PostRequest($"https://gateway.tianwayun.com/apps/course/stu/selectTask/selectTimeCourse?taskId={GlobalTaskId}&classIds={GlobalClassId}", AccessToken, AppKey);

                        Console.WriteLine($"The {times}st try.");
                        Console.WriteLine(back);
                        Console.WriteLine("====================================");
                        await Task.Delay(delayTime);
                        times++;
                    }
                    catch (HttpRequestException httpEx)
                    {
                        Console.WriteLine("============Network Error============");
                        Console.WriteLine($"捕获到到由于数据包发送失败造成的{httpEx.Message}异常: ");
                        Console.WriteLine(httpEx.ToString());
                        Console.WriteLine("如果多次发生该问题，请检查您的网络是否正常。");
                        Console.WriteLine("============Network Error============");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("============Fatal Error!============");
                        Console.WriteLine(ex.ToString());
                        Console.WriteLine("为防止发生不可预期的问题，程序将自动退出。");
                        Console.WriteLine("============Fatal Error!============");
                        break;
                    }

                }
                break;
            case 1:
                int tryBefore;
            INPUT_TRYBEFORE: Console.WriteLine("请输入在任务开始前多少秒开始尝试选课(单位: 秒, 只接受整数), 建议在5左右: ");
                if (!int.TryParse(Console.ReadLine(), out tryBefore))
                {
                    Console.WriteLine("无法将您的输入转换为整数。");
                    goto INPUT_TRYBEFORE;
                }
            INPUT_RESTTIME: Console.WriteLine("请输入在开始尝试选课后的尝试间隔(单位: ms, 只输入整数, 1s = 1000ms)。建议在100左右: ");
                int restTime;
                if (!int.TryParse(Console.ReadLine(), out restTime))
                {
                    Console.WriteLine("无法将您的输入转换为整数。");
                    goto INPUT_RESTTIME;
                }
                DateTime StartTryingTime = GlobalStartTime.AddSeconds(-tryBefore);
                Console.WriteLine($"选课任务开始时间: {GlobalStartTime.ToLocalTime()}");
                Console.WriteLine($"将在 {StartTryingTime.ToLocalTime()} 时开始尝试抢课。");

                ExecuteAt(StartTryingTime, async () =>
                {
                    Console.WriteLine("===============抢课已开始===============");
                    AccessToken = await Login(AccountName, Password);
                    Console.WriteLine("令牌已刷新。");
                    int times = 1;
                    while (true)
                    {
                        try
                        {
                            string back = await PostRequest($"https://gateway.tianwayun.com/apps/course/stu/selectTask/selectTimeCourse?taskId={GlobalTaskId}&classIds={GlobalClassId}", AccessToken, AppKey);

                            Console.WriteLine($"The {times}st try.");
                            Console.WriteLine(back);
                            Console.WriteLine("====================================");
                            await Task.Delay(restTime);
                            times++;
                        }
                        catch (HttpRequestException httpEx)
                        {
                            Console.WriteLine("============Network Error============");
                            Console.WriteLine($"捕获到到由于数据包发送失败造成的{httpEx.Message}异常: ");
                            Console.WriteLine(httpEx.ToString());
                            Console.WriteLine("如果多次发生该问题，请检查您的网络是否正常。");
                            Console.WriteLine("============Network Error============");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("============Fatal Error!============");
                            Console.WriteLine(ex.ToString());
                            Console.WriteLine("============Fatal Error!============");
                            break;
                        }

                    }
                });
                Console.WriteLine("\n===============定时模式已启用 请勿关闭命令行窗口===============\n");
                await Task.Delay(Timeout.Infinite);
                break;
        }
        Console.WriteLine("按回车键退出程序。");
        Console.ReadLine();
    }

    /// <summary>
    /// 向服务器发送POST请求
    /// </summary>
    /// <param name="url">指定URL</param>
    /// <param name="AccessToken">指定AccessToken</param>
    /// <param name="AppKey">指定AppKey</param>
    /// <returns></returns>
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

    public static async Task<string> GetRequest(string url)
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("SelectCourse");
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode(); // Throw if not a success code.

            string content = await response.Content.ReadAsStringAsync();
            return content;
        }
    }

    public static string ConvertUtcToBeijingTime(string utcTime)
    {
        // 解析 UTC 时间字符串
        DateTime utcDateTime = DateTime.Parse(utcTime, null, System.Globalization.DateTimeStyles.RoundtripKind);

        // 获取北京时区信息
        TimeZoneInfo beijingTimeZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");

        // 将 UTC 时间转换为北京时间
        DateTime beijingDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, beijingTimeZone);

        // 格式化为 "年/月/日 时:分:秒"
        string formattedDateTime = beijingDateTime.ToString("yyyy/MM/dd HH:mm:ss");

        return formattedDateTime;
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
                if (json["msgcode"].ToString() == "EXECUTE_SECURITY_ASSESS")
                {
                    Console.WriteLine("\nError: 登录失败。\n 天蛙云要求您先更改安全级别更高的密码。请先前往 https://tianwayun.com/ 手动登录账户并按照提示修改密码后再使用本软件登录。\n服务器Response:\n");
                    Console.WriteLine(r);
                    Console.WriteLine("按任意键退出程序");
                    Console.ReadLine();
                    Environment.Exit(1);
                }
                Console.WriteLine("\nError: 登录失败。请检查用户名与密码。服务器Response:\n");
                Console.WriteLine(r);
                Console.WriteLine("按任意键退出程序");
                Console.ReadLine();
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
    private static async Task InquireSelectTargetCourse()
    {
        string url = $"https://gateway.tianwayun.com/apps/course/stu/selectTask/getByTimeTaskId?taskId={GlobalTaskId}";
        string r;
        List<string> className = new List<string>();
        List<string> teacherName = new List<string>();
        List<string> classId = new List<string>();
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
            Console.WriteLine("按任意键退出程序");
            Console.ReadLine();
            System.Environment.Exit(1);

        }
        Console.WriteLine("==============================================");
        int UserListCount = json["data"]["courseDTOS"][0]["classDTOS"].Count();
        for (int i = 0; i < UserListCount; i++)
        {
            className.Add(json["data"]["courseDTOS"][0]["classDTOS"][i]["className"].ToString());
            teacherName.Add(json["data"]["courseDTOS"][0]["classDTOS"][i]["teacherName"].ToString());
            classId.Add(json["data"]["courseDTOS"][0]["classDTOS"][i]["classId"].ToString());
            Console.WriteLine($"[{i + 1}]\n | 课程名称:{className[i]}");
            Console.WriteLine($" | 授课教师:{teacherName[i]}");
            Console.WriteLine($" | ClassID:{classId[i]}");
            Console.WriteLine();

        }
        Console.WriteLine("==============================================");
        Console.WriteLine("请选择课程，并输入课程前的序号: ");
        int select = int.Parse(Console.ReadLine());
        GlobalClassId = classId[select - 1];

        Console.WriteLine($"成功, 当前已选择课程: {className[select - 1]}。");
    }
    private static async Task InquireSelectTargetTask()
    {
        string url = "https://gateway.tianwayun.com/apps/course/stu/selectTask/list?endFlag=false";
        string r;
        List<string> taskName = new List<string>();
        List<string> semesterName = new List<string>();
        List<string> beginTime = new List<string>();
        List<string> beginTimeStamp = new List<string>();
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
            Console.WriteLine("按任意键退出程序");
            Console.ReadLine();
            System.Environment.Exit(1);

        }
        Console.WriteLine("==============================================");
        var rows = json["data"]["rows"];
        for (int i = 0; i < rows.Count(); i++)
        {
            taskName.Add(rows[i]["taskName"].ToString());
            semesterName.Add(rows[i]["semesterName"].ToString());
            beginTime.Add(rows[i]["beginTimeStr"].ToString());
            beginTimeStamp.Add(rows[i]["beginTimeStamp"].ToString());
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
        GlobalStartTime = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(beginTimeStamp[select - 1])).LocalDateTime;
        Console.WriteLine($"成功, 当前选课任务为: {taskName[select - 1]}。");
    }

    private static void ExecuteAt(DateTime targetTime, Func<Task> action)
    {
        TimeSpan delay = targetTime - DateTime.Now;

        if (delay <= TimeSpan.Zero)
        {
            // 如果目标时间已经过去，立即执行
            action();
            return;
        }

        Timer timer = null;
        timer = new Timer(_ =>
        {
            action();
            // 释放定时器资源
            timer.Dispose();
        }, null, delay, Timeout.InfiniteTimeSpan);
    }
}