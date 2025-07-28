import Banner from '@common/components/Banner';
import useMounted from '@common/hooks/useMounted';
import Link from '@frontend/components/Link';
import {
  useCookies,
  userTestingBannerVersion,
} from '@frontend/hooks/useCookies';
import React from 'react';

export default function UserTestingBanner() {
  const { isMounted } = useMounted();
  const { getCookie, setUserTestingBannerSeenCookie } = useCookies();

  const userTestingCookieValue = getCookie('userTestingBannerSeen');

  const isUserTestingBannerSeen = userTestingCookieValue
    ? JSON.parse(userTestingCookieValue).version === userTestingBannerVersion
    : false;

  if (isMounted && !isUserTestingBannerSeen) {
    return (
      <Banner
        label="user-testing-banner"
        onClose={() => setUserTestingBannerSeenCookie(userTestingBannerVersion)}
      >
        <p className="govuk-!-font-weight-bold govuk-!-margin-bottom-0">
          Shape the future of Explore education statistics
        </p>
        <p className="govuk-!-margin-bottom-2">
          <Link
            to="https://forms.office.com/e/QVWqCbbxCj"
            target="_blank"
            rel="noopener noreferrer"
          >
            Share your feedback to help us improve this service
          </Link>
        </p>
      </Banner>
    );
  }
  return null;
}
