# -*- coding: utf-8 -*-
"""
Created on Thu Feb 21 14:32:10 2019

@author: MSI-Gaming
"""


import subprocess
import sys
import os
#cmd :'ffmpeg -i D:\SITP\GD\测试文件\ss\Y3800002282-Y3800002283.avi -q:v 2 D:\SITP\GD\测试文件\ss\Y3800002282-Y3800002283\%4d.jpg'
#ffmpeg -i D:\SITP\GD\测试文件\ss\Y3800002282-Y3800002283.avi -vf select='eq(pict_type\,I)' -vsync 2 -q:v 2  D:\SITP\GD\测试文件\ss\new\%06d.jpg
from subprocess import call
import win32pipe, win32file, pywintypes
def mkdir(path):
    # 去除首位空格
    path=path.strip()
    # 去除尾部 \ 符号
    path=path.rstrip("\\")
    iE=os.path.exists(path)
    if not iE:
        os.makedirs(path)
    else:
        print('Already exists!')   
def Cut_Fram(file_video,output_file):
    pipename=r'\\.\pipe\cutfram_result1'
    #####管道部分
    print("pipe client")
    quit = False
    count=0
    while not quit:
        try:
            handle = win32file.CreateFile(
                pipename,
                win32file.GENERIC_READ | win32file.GENERIC_WRITE,
                0,
                None,
                win32file.OPEN_EXISTING,
                0,
                None
            )

            while count<1:
                
    ########
                
                s_1=file_video.rindex('\\')
                e_1=file_video.rindex('.')
                v_name=file_video[s_1:e_1]
                o_v_file=output_file+'\\'+v_name
                mkdir(o_v_file)
                bigimg_file=o_v_file+'\\bigimg'
                smallimg_file=o_v_file+'\\smallimg'
                mkdir(bigimg_file)
                mkdir(smallimg_file)
                cmd_l_bigimg='ffmpeg -i '+file_video+' -q:v 2 -threads 1 '+bigimg_file+'\\%6d.jpg '
                cmd_l_smallimg='ffmpeg -threads 1 -i '+file_video+' -s 9x8 '+smallimg_file+'\\%6d.jpg'
                fdp_bigimg = subprocess.Popen( cmd_l_bigimg, shell=True)
                fdp_small = subprocess.Popen( cmd_l_smallimg, shell=True)
                wait_final=fdp_bigimg.wait()
                win32file.WriteFile(handle, str.encode(str('4_Done')))
                count=count+1
        except count>0:
            quit = True
if __name__ == '__main__':
    #call(['setx Path "D:\SITP\ffmpeg-20190225-2e67f75-win64-static"'])
    file_video=sys.argv[1]
    output_file=sys.argv[2]
    #output_file='D:\\SITP\\GD\\video\\2nd\\try'
    #file_video='D:\\SITP\\GD\\video\\2nd\\try\\Y3800002282-Y3800002283.avi'
    Cut_Fram(file_video,output_file)