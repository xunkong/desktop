// Xunkong.Desktop.RegistryInvoker.cpp : 此文件包含 "main" 函数。程序执行将在此处开始并结束。
//
//#pragma comment(linker,"/subsystem:\"Windows\" /entry:\"mainCRTStartup\"")
#include <iostream>
#include <Windows.h>

char ByteToChar(byte n)
{
	if (n < 10)
	{
		return n + 48;
	}
	else
	{
		return n + 55;
	}
}

byte CharToByte(char n)
{
	if (n < 58)
	{
		return n - 48;
	}
	else
	{
		return n - 55;
	}
}

int main(int argc, char* argv[])
{
	if (argc < 2)
	{
		std::cout << "Invalid arguement.";
		exit(-1);
	}
	bool getADLPROD = false;
	bool setADLPROD = false;
	bool isSea = false;
	char ADLPRODHexString[8192] = { 0 };
	for (size_t i = 0; i < argc; i++)
	{
		if (!strcmp(argv[i], "GetADLPROD"))
		{
			getADLPROD = true;
		}
		if (!strcmp(argv[i], "SetADLPROD"))
		{
			setADLPROD = true;
		}
		if (!strcmp(argv[i], "--isSea"))
		{
			isSea = true;
		}
		if (!strcmp(argv[i], "--ADLPROD"))
		{
			if (i + 1 < argc)
			{
				memcpy(ADLPRODHexString, argv[i + 1], strlen(argv[i + 1]));
			}
		}
	}
	if (!(getADLPROD ^ setADLPROD))
	{
		std::cout << "Invalid arguement.";
		exit(-1);
	}

	LPCWSTR lpSubKey;
	LPCWSTR lpValue;
	if (isSea)
	{
		lpSubKey = L"Software\\miHoYo\\Genshin Impact";
		lpValue = L"MIHOYOSDK_ADL_PROD_OVERSEA_h1158948810";
	}
	else
	{
		lpSubKey = L"Software\\miHoYo\\原神";
		lpValue = L"MIHOYOSDK_ADL_PROD_CN_h3123967166";
	}


	if (getADLPROD)
	{
		byte pData[8192] = { 0 };
		DWORD length = 8192;
		auto stats = RegGetValue(HKEY_CURRENT_USER, lpSubKey, lpValue, RRF_RT_ANY, nullptr, pData, &length);
		if (stats != ERROR_SUCCESS)
		{
			std::cout << "Cannot get ADLPROD for current user (ErrorCode: " << stats << ")";
			exit(-1);
		}
		else
		{
			char* hexString = new char[length * 2 + 1];
			for (size_t i = 0; i < length; i++)
			{
				byte height = pData[i] >> 4;
				hexString[i * 2] = ByteToChar(height);
				byte low = pData[i] & 0x0F;
				hexString[i * 2 + 1] = ByteToChar(low);
			}
			hexString[2 * length] = '\0';
			std::cout << hexString;
		}
	}

	if (setADLPROD)
	{
		if (strlen(ADLPRODHexString) == 0 || strlen(ADLPRODHexString) % 2 != 0)
		{
			std::cout << "Invalid arguement.";
			exit(-1);
		}
		DWORD len = strlen(ADLPRODHexString) / 2;
		byte pData[8192] = { 0 };
		for (size_t i = 0; i < len; i++)
		{
			byte height = CharToByte(ADLPRODHexString[i * 2]);
			byte low = CharToByte(ADLPRODHexString[i * 2 + 1]);
			pData[i] = height * 16 + low;
		}

		auto stats = RegSetKeyValue(HKEY_CURRENT_USER, lpSubKey, lpValue, REG_BINARY, pData, len);
		if (stats != ERROR_SUCCESS)
		{
			std::cout << "Cannot set ADLPROD for current user (ErrorCode: " << stats << ")";
			exit(-1);
		}
		else
		{
			std::cout << "00";
			exit(0);
		}
	}
}

// 运行程序: Ctrl + F5 或调试 >“开始执行(不调试)”菜单
// 调试程序: F5 或调试 >“开始调试”菜单

// 入门使用技巧: 
//   1. 使用解决方案资源管理器窗口添加/管理文件
//   2. 使用团队资源管理器窗口连接到源代码管理
//   3. 使用输出窗口查看生成输出和其他消息
//   4. 使用错误列表窗口查看错误
//   5. 转到“项目”>“添加新项”以创建新的代码文件，或转到“项目”>“添加现有项”以将现有代码文件添加到项目
//   6. 将来，若要再次打开此项目，请转到“文件”>“打开”>“项目”并选择 .sln 文件
