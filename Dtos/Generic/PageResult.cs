﻿namespace HealthTracker.api.Dtos.Generic
{
    public class PageResult<T> : Result<List<T>>
    {
        public int Page { get; set; }
        public int ResultCount { get; set; }
        public int ResultPerPage { get; set; }

    }
}
