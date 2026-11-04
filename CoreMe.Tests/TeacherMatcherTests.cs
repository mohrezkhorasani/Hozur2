using Application.General;
using Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CoreMe.Tests
{
    public class TeacherMatcherTests
    {
        [Theory]
        [InlineData("حجت الاسلام والمسلمین دکتر سید علی حسینی", "علی حسینی")]
        [InlineData("حجت‌الاسلام والمسلمین علی حسینی", "علی حسینی")]
        [InlineData("حجت اسلام علی حسینی", "علی حسینی")]
        [InlineData("حجة الاسلام و المسلمین علی حسینی", "علی حسینی")]
        [InlineData("حجه الاسلام علی حسینی", "علی حسینی")]
        [InlineData("آیت الله العظمی جوادی آملی", "جوادی املی")]
        [InlineData("آیت‌الله جوادی آملی", "جوادی املی")]
        [InlineData("ایت الله جوادی آملی", "جوادی املی")]
        [InlineData("دکتر رضا محمدی", "رضا محمدی")]
        [InlineData("دكتر رضا محمدي", "رضا محمدی")]
        [InlineData("مهندس علیرضا رضایی", "علیرضا رضایی")]
        [InlineData("سید مهدی میرباقری", "مهدی میرباقری")]
        [InlineData("جناب آقای دکتر احمدی", "احمدی")]
        public void StripHonorifics_ShouldCorrectlyStripTitles(string input, string expectedCore)
        {
            var stripped = TeacherNameMatcher.StripHonorifics(input);
            Assert.Equal(expectedCore, stripped);
        }

        [Fact]
        public void ExtractPosition_ShouldExtractCorrectly()
        {
            var (name, pos) = TeacherNameMatcher.ExtractPosition("علیرضا حسینی(سردبیر روزنامه حکمت)");
            Assert.Equal("علیرضا حسینی", name);
            Assert.Equal("سردبیر روزنامه حکمت", pos);

            var (name2, pos2) = TeacherNameMatcher.ExtractPosition("دکتر حسینی（رئیس پژوهشگاه）");
            Assert.Equal("دکتر حسینی", name2);
            Assert.Equal("رئیس پژوهشگاه", pos2);

            var (name3, pos3) = TeacherNameMatcher.ExtractPosition("علی حسینی");
            Assert.Equal("علی حسینی", name3);
            Assert.Null(pos3);
        }

        [Fact]
        public void NormalizePersianText_ShouldHandleTashkeelAndDigits()
        {
            var input = "حُجَّتُ الإِسْلامِ دُکْتُر عَلِیّ ۱۴۰۵";
            var normalized = TeacherNameMatcher.NormalizePersianText(input);
            Assert.Equal("حجت الاسلام دکتر علی 1405", normalized);
        }

        [Fact]
        public void ParseJalaliDateTime_ShouldParseCorrectly()
        {
            var (success, utcDt, error) = TeacherNameMatcher.ParseJalaliDateTime("1405/08/13", "10:30");
            Assert.True(success);
            Assert.Empty(error);

            // 1405/08/13 in Gregorian is 2026-11-04.
            // 10:30 Tehran time (UTC+03:30) is 07:00 UTC.
            Assert.Equal(2026, utcDt.Year);
            Assert.Equal(11, utcDt.Month);
            Assert.Equal(4, utcDt.Day);
            Assert.Equal(7, utcDt.Hour);
            Assert.Equal(0, utcDt.Minute);
        }

        [Fact]
        public void ParseJalaliDateTime_WithPersianDigits_ShouldParseCorrectly()
        {
            var (success, utcDt, error) = TeacherNameMatcher.ParseJalaliDateTime("۱۴۰۵/۰۸/۱۳", "۱۰:۳۰");
            Assert.True(success);
            Assert.Empty(error);
            Assert.Equal(2026, utcDt.Year);
            Assert.Equal(11, utcDt.Month);
            Assert.Equal(4, utcDt.Day);
            Assert.Equal(7, utcDt.Hour);
        }

        [Fact]
        public void FindMatchingTeacher_ShouldMatchTeacher_AndIgnoreNonTeachers()
        {
            var dbUsers = new List<UserTBL>
            {
                new UserTBL
                {
                    ID = 100,
                    Firstname = "علی",
                    LastName = "حسینی",
                    Type = MainEntity_Types.USER_USER // Regular user, NOT a teacher
                },
                new UserTBL
                {
                    ID = 101,
                    Firstname = "دکتر رضا",
                    LastName = "محمدی",
                    Type = MainEntity_Types.USER_TEACHER // Valid teacher
                },
                new UserTBL
                {
                    ID = 102,
                    Firstname = "سید مهدی",
                    LastName = "میرباقری",
                    Type = MainEntity_Types.USER_TEACHER // Valid teacher
                }
            };

            // 1. Should NOT match ID 100 because he is a regular user, not a teacher
            var matchNormal = TeacherNameMatcher.FindMatchingTeacher("حجت الاسلام علی حسینی", dbUsers);
            Assert.Null(matchNormal);

            // 2. Should match ID 101 despite varying titles
            var match1 = TeacherNameMatcher.FindMatchingTeacher("حجت الاسلام والمسلمین آقای رضا محمدی", dbUsers);
            Assert.NotNull(match1);
            Assert.Equal(101, match1.ID);

            // 3. Should match ID 102
            var match2 = TeacherNameMatcher.FindMatchingTeacher("آیت الله سید مهدی میرباقری(استاد حوزه)", dbUsers);
            Assert.NotNull(match2);
            Assert.Equal(102, match2.ID);
        }

        [Fact]
        public void MatchAndFormatTeachers_ShouldFormatMultipleWithPositions()
        {
            var dbUsers = new List<UserTBL>
            {
                new UserTBL
                {
                    ID = 201,
                    Firstname = "علی",
                    LastName = "احمدی",
                    Type = MainEntity_Types.USER_TEACHER
                }
            };

            var input = "حجت الاسلام والمسلمین علی احمدی(دبیر علمی)، دکتر مهدی حسینی(سردبیر)";
            var results = TeacherNameMatcher.MatchAndFormatTeachers(input, dbUsers);

            Assert.Equal(2, results.Count);

            // First teacher matched in DB
            Assert.True(results[0].IsMatchedInDb);
            Assert.Equal(201, results[0].Id);
            Assert.Equal("علی احمدی", results[0].Name);
            Assert.Equal("دبیر علمی", results[0].Position);
            Assert.Equal("علی احمدی(دبیر علمی)", results[0].FormattedDisplayName);

            // Second teacher not in DB
            Assert.False(results[1].IsMatchedInDb);
            Assert.Equal(0, results[1].Id);
            Assert.Equal("سردبیر", results[1].Position);
            Assert.Equal("دکتر مهدی حسینی(سردبیر)", results[1].FormattedDisplayName);
        }
    }
}
