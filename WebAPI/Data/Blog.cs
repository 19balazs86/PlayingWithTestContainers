﻿using NpgsqlTypes;

namespace WebAPI.Data;

public readonly record struct BlogId(int Value)
{
    public static BlogId Create(int value) => new BlogId(value);

    public static implicit operator int(BlogId blogId) => blogId.Value;

    //public static explicit operator BlogId(int value) => new BlogId(value);

    public override string ToString() => Value.ToString();
}

public sealed class Blog : BaseEntityWithId<BlogId>
{
    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public MemberId OwnerId { get; set; }

    public Member? Owner { get; set; } = default!;

    public List<string> Tags { get; set; } = [];

    public NpgsqlTsVector FullTextSearchVector { get; set; } = default!;
}