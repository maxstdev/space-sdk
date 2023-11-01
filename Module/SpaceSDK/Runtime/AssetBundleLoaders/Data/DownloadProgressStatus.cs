namespace MaxstXR.Place
{
	public struct DownloadProgressStatus
	{
		public long downloadedBytes;
		public long totalBytes;
		public long remainedBytes;
		public float percent;

		public DownloadProgressStatus(long downloadedBytes, long totalBytes, long remainedBytes, float percent)
		{
			this.downloadedBytes = downloadedBytes;
			this.totalBytes = totalBytes;
			this.remainedBytes = remainedBytes;
			this.percent = percent;
		}
	}
}

