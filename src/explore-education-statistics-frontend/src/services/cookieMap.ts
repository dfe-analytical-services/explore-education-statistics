function daysFromNow(days = 0) {
  const result = new Date();
  if (!days) {
    // Add one month
    result.setMonth([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 0][result.getMonth()]);
  } else {
    result.setDate(result.getDate() + days);
  }
  return result;
}

const CookieMap = {
  bannerSeenCookie: {
    name: 'ees_banner_seen',
    expires: daysFromNow(),
    duration: '1 month',
  },
  disableGACookie: {
    name: 'ees_disable_google_analytics',
    expires: daysFromNow(),
    duration: '1 month',
  },
};

export default CookieMap;
