﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmLight4.Common
{
    class CmdHelper
    {
        private static string CmdPath = @"C:\Windows\System32\cmd.exe";

        /// <summary>
        /// 执行cmd命令
        /// 多命令请使用批处理命令连接符：
        /// </summary>
        /// <param name="cmd">要执行的命令</param>
        public static int RunCmd(string cmd)
        {
            cmd = cmd.Trim().TrimEnd('&') + "&exit";//说明：不管命令是否成功均执行exit命令，否则当调用ReadToEnd()方法时，会处于假死状态
            using (Process p = new Process())
            {
                p.StartInfo.FileName = CmdPath;
                p.StartInfo.UseShellExecute = false;        //是否使用操作系统shell启动
                p.StartInfo.RedirectStandardInput = true;   //接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardOutput = true;  //由调用程序获取输出信息
                p.StartInfo.RedirectStandardError = true;   //重定向标准错误输出
                p.StartInfo.CreateNoWindow = true;          //不显示程序窗口
                p.Start();//启动程序

                //向cmd窗口写入命令
                p.StandardInput.WriteLine(cmd);
                p.StandardInput.AutoFlush = true;
                return p.Id;
                //获取cmd窗口的输出信息
                //output = p.StandardOutput.ReadToEnd();
                //p.WaitForExit();//等待程序执行完退出进程
                //p.Close();
            }
        }

        /// <summary>
        /// 执行Cmd命令
        /// </summary>
        /// <param name="cmd">命令</param>
        /// <param name="arguments">参数</param>
        /// <returns></returns>
        public static Process RunProcess(string cmd,string arguments="")
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = cmd;
                if (!string.IsNullOrEmpty(arguments))
                {
                    process.StartInfo.Arguments = arguments;
                }
                //设置不在新窗口中启动新的进程
                process.StartInfo.CreateNoWindow = true;
                //不使用操作系统使用的shell启动进程
                process.StartInfo.UseShellExecute = false;
                //将输出信息重定向
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;   //重定向标准错误输出
                process.StartInfo.RedirectStandardInput = true;   //接受来自调用程序的输入信息
                return process;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }
    }
}
