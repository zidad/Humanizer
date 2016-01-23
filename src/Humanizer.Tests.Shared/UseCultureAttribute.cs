using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading;
using Xunit.Sdk;

namespace Humanizer.Tests
{
   
    /// <summary>
    /// Apply this attribute to your test method to replace the
    /// <see cref="Thread.CurrentThread" /> <see cref="CultureInfo.CurrentCulture" /> and
    /// <see cref="CultureInfo.CurrentUICulture" /> with another culture.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class UseCultureAttribute : BeforeAfterTestAttribute
    {
        readonly Lazy<CultureInfo> culture;
        readonly Lazy<CultureInfo> uiCulture;

        CultureInfo originalCulture;
        CultureInfo originalUICulture;

        [ThreadStatic]
        static CultureInfo nullCulture;

        [ThreadStatic]
        static CultureInfo nullUICulture;

        static readonly AsyncLocal<CultureInfo> currentCultureFlowing = new AsyncLocal<CultureInfo>(
            args =>
            {
                if (args.PreviousValue == null)
                {
                    nullCulture = CultureInfo.CurrentCulture;
                }

                if (args.CurrentValue == null)
                {
                    CultureInfo.CurrentCulture = nullCulture;
                    nullCulture = null;
                }
                else
                {
                    CultureInfo.CurrentCulture = args.CurrentValue;
                }
            });

        static readonly AsyncLocal<CultureInfo> currentUICultureFlowing = new AsyncLocal<CultureInfo>(
            args =>
            {
                if (args.PreviousValue == null)
                {
                    nullUICulture = CultureInfo.CurrentUICulture;
                }

                if (args.CurrentValue == null)
                {
                    CultureInfo.CurrentUICulture = nullUICulture;
                    nullUICulture = null;
                }
                else
                {
                    CultureInfo.CurrentUICulture = args.CurrentValue;
                }
            });



        /// <summary>
        /// Replaces the culture and UI culture of the current thread with
        /// <paramref name="culture" />
        /// </summary>
        /// <param name="culture">The name of the culture.</param>
        /// <remarks>
        /// <para>
        /// This constructor overload uses <paramref name="culture" /> for both
        /// <see cref="Culture" /> and <see cref="UICulture" />.
        /// </para>
        /// </remarks>
        public UseCultureAttribute(string culture)
            : this(culture, culture)
        { }

        /// <summary>
        /// Replaces the culture and UI culture of the current thread with
        /// <paramref name="culture" /> and <paramref name="uiCulture" />
        /// </summary>
        /// <param name="culture">The name of the culture.</param>
        /// <param name="uiCulture">The name of the UI culture.</param>
        public UseCultureAttribute(string culture, string uiCulture)
        {
            this.culture = new Lazy<CultureInfo>(() => new CultureInfo(culture));
            this.uiCulture = new Lazy<CultureInfo>(() => new CultureInfo(uiCulture));
        }

        /// <summary>
        /// Gets the culture.
        /// </summary>
        public CultureInfo Culture => culture.Value;

        /// <summary>
        /// Gets the UI culture.
        /// </summary>
        public CultureInfo UICulture => uiCulture.Value;

        /// <summary>
        /// Stores the current <see cref="CultureInfo.CurrentCulture" />
        /// <see cref="CultureInfo.CurrentCulture" /> and <see cref="CultureInfo.CurrentUICulture" />
        /// and replaces them with the new cultures defined in the constructor.
        /// </summary>
        /// <param name="methodUnderTest">The method under test</param>
        public override void Before(MethodInfo methodUnderTest)
        {
            originalCulture = CultureInfo.CurrentCulture;
            originalUICulture = CultureInfo.CurrentUICulture;


            currentCultureFlowing.Value = Culture;
            currentUICultureFlowing.Value = UICulture;
        }

        /// <summary>
        /// Restores the original <see cref="CultureInfo.CurrentCulture" /> and
        /// <see cref="CultureInfo.CurrentUICulture" /> to <see cref="CultureInfo.CurrentCulture" />
        /// </summary>
        /// <param name="methodUnderTest">The method under test</param>
        public override void After(MethodInfo methodUnderTest)
        {
            currentCultureFlowing.Value = originalCulture;
            currentUICultureFlowing.Value = originalUICulture;
        }
    }


}
