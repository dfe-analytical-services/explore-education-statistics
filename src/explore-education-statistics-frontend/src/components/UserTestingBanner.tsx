import Banner from '@common/components/Banner';
import { useTenant } from '@common/contexts/TenantContext';
import useMounted from '@common/hooks/useMounted';
import Link from '@frontend/components/Link';
import { PageWidth } from '@frontend/components/Page';
import {
  useCookies,
  userTestingBannerVersion,
} from '@frontend/hooks/useCookies';
import classNames from 'classnames';
import React from 'react';

interface Props {
  width?: PageWidth;
}

export default function UserTestingBanner({ width }: Props) {
  const { userResearchBanner } = useTenant();
  const { isMounted } = useMounted();
  const { getCookie, setUserTestingBannerSeenCookie } = useCookies();

  const userTestingCookieValue = getCookie('userTestingBannerSeen');

  const isUserTestingBannerSeen = userTestingCookieValue
    ? JSON.parse(userTestingCookieValue).version === userTestingBannerVersion
    : false;

  if (userResearchBanner && isMounted && !isUserTestingBannerSeen) {
    return (
      <Banner
        className={classNames({
          'dfe-width-container--wide': width === 'wide',
          'dfe-width-container--full': width === 'full',
        })}
        label="Feedback banner"
        onClose={() => setUserTestingBannerSeenCookie(userTestingBannerVersion)}
      >
        <p className="govuk-!-font-weight-bold govuk-!-margin-bottom-0">
          {userResearchBanner.title}
        </p>
        <p className="govuk-!-margin-bottom-2">
          <Link
            to={userResearchBanner.url}
            target="_blank"
            rel="noopener noreferrer"
          >
            {userResearchBanner.linkText} (opens in new tab)
          </Link>
        </p>
      </Banner>
    );
  }
  return null;
}
