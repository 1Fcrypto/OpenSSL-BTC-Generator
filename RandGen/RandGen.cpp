#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <openssl/evp.h>
#include <openssl/ec.h>
#include <openssl/bn.h>
#include <openssl/rand.h>
#include <openssl/sha.h>
#include "uint256.h"
#include <mutex>

void RandAddSeed(bool fPerfmon = false);
__int64 GetTime();


typedef void DlgCallback(const unsigned char* key, size_t size_key, size_t inx);

std::vector<DlgCallback*>* _callback = 0;
std::mutex m_callback, m_init;

void send_callback(const unsigned char* key, size_t size_key, size_t inx)
{
	std::unique_lock<std::mutex> lk(m_callback);
	if (_callback != 0)
		for (int i = 0; i < _callback->size(); i++)
			_callback->at(i)(key, size_key, inx);
}

void run(const size_t total_keys, const char* out_file, bool flushToConsole = false)
{
	BN_CTX* bn_ctx = BN_CTX_new();
	BIGNUM* curve_order = BN_new();
	BIGNUM* priv_key = BN_new();
	EC_KEY* ec_key = 0;
	{
		std::unique_lock<std::mutex> lk(m_init);
		ec_key = EC_KEY_new_by_curve_name(NID_secp256k1);
		const EC_GROUP* group = EC_KEY_get0_group(ec_key);
		EC_GROUP_get_order(group, curve_order, bn_ctx);
	}
	FILE* file = 0;
	if (out_file != 0)
		file = fopen(out_file, "a");

	for (size_t n = 0; n < total_keys; n++) {
		BN_rand_range(priv_key, curve_order);
		unsigned char bytes[32];
		BN_bn2bin(priv_key, bytes);
		if (out_file != 0)
			fprintf(file, "0x");
		char* res = new char[255];
		int pos = 0;
		for (int i = 0; i < 32; i++) {
			if (out_file != 0)
				fprintf(file, "%02x", bytes[i]);
			if (flushToConsole)
				printf("%02x", bytes[i]);
		}
		send_callback(bytes, 32, n);
		delete[] res;

		if (out_file != 0)
			fprintf(file, "\n");
		if (flushToConsole && n % 10000 == 0) {
			printf("\rcompleted: %lf %%", (double)n / (double)total_keys * 100.0);
			fflush(stdout);
		}
	}

	if (out_file != 0)
		fclose(file);

	EC_KEY_free(ec_key);
	BN_CTX_free(bn_ctx);
	BN_free(curve_order);
	BN_free(priv_key);

}

extern "C" __declspec(dllexport) void SetCallback(DlgCallback callback, bool add)
{
	std::unique_lock<std::mutex> lk(m_callback);
	if (add)
		_callback->push_back(callback);
	else
	{
		for (auto i = _callback->begin(); i != _callback->end(); i++)
		{
			if ((*i) == callback) {
				_callback->erase(i);
				break;
			}
		}
	}
}


extern "C" __declspec(dllexport) void Init()
{
	std::unique_lock<std::mutex> lk(m_callback);
	_callback = new std::vector<DlgCallback*>();
	// Seed random number generator with screen scrape and other hardware sources
	RAND_screen();

	// Seed random number generator with perfmon data
	RandAddSeed(true);
}

extern "C" __declspec(dllexport) void Run(size_t total_keys, const char* pathToFile, bool flushToConsole)
{
	run(total_keys, pathToFile, flushToConsole);
}

int main(int argc, char** argv)
{
	if (argc != 3) {
		printf("usage:");
		printf("%s num_keys out_file", argv[0]);
		exit(-1);
	}
	size_t total_keys = atoll(argv[1]);
	const char* out_file = argv[2];
	run(total_keys, out_file, true);
	return 0;
}


void RandAddSeed(bool fPerfmon)
{
	// Seed with CPU performance counter
	LARGE_INTEGER PerformanceCount;
	QueryPerformanceCounter(&PerformanceCount);
	RAND_add(&PerformanceCount, sizeof(PerformanceCount), 1.5);
	memset(&PerformanceCount, 0, sizeof(PerformanceCount));

	static __int64 nLastPerfmon;
	if (fPerfmon || GetTime() > nLastPerfmon + 5 * 60)
	{
		nLastPerfmon = GetTime();

		// Seed with the entire set of perfmon data
		unsigned char* pdata = new unsigned char[250000];
		memset(pdata, 0, sizeof(pdata));
		unsigned long nSize = sizeof(pdata);
		long ret = RegQueryValueEx(HKEY_PERFORMANCE_DATA, L"Global", NULL, NULL, pdata, &nSize);
		RegCloseKey(HKEY_PERFORMANCE_DATA);
		if (ret == ERROR_SUCCESS)
		{
			uint256 hash;
			SHA256(pdata, nSize, (unsigned char*)&hash);
			RAND_add(&hash, sizeof(hash), min(nSize / 500.0, (double)sizeof(hash)));
			hash = 0;
			memset(pdata, 0, nSize);
			printf("RandAddSeed() got %d bytes of performance data\n", nSize);
		}
		delete[] pdata;
	}
}


__int64 GetTime()
{
	return time(NULL);
}