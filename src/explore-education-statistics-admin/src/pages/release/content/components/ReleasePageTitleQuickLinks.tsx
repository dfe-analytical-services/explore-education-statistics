import React from 'react';
import Link from '@admin/components/Link';
import { ContentPublication } from '@admin/services/publicationService';
import { EditableRelease } from '@admin/services/releaseContentService';
import releaseFileService from '@admin/services/releaseFileService';
import ButtonText from '@common/components/ButtonText';
import { useMobileMedia } from '@common/hooks/useMedia';
import styles from '@common/modules/release/components/ReleasePageTitleQuickLinks.module.scss';

interface Props {
  publication: ContentPublication;
  release: EditableRelease;
  showSubscriptionLink?: boolean;
}

const ReleasePageTitleQuickLinks = ({
  publication,
  release,
  showSubscriptionLink = true,
}: Props) => {
  const { isMedia: isMobileMedia } = useMobileMedia();

  return (
    <div className={isMobileMedia ? '' : 'govuk-grid-column-one-third'}>
      <h2 className={styles.quickLinksHeading} id="quick-links">
        Quick links
      </h2>
      <nav
        role="navigation"
        aria-labelledby="quick-links"
        data-testid="quick-links"
      >
        <ul
          className={`${styles.quickLinks} govuk-list dfe-flex dfe-flex-direction--column dfe-gap-2`}
        >
          <li>
            <ButtonText
              preventDoubleClick
              onClick={() => releaseFileService.downloadFilesAsZip(release.id)}
            >
              Download all data (ZIP)
            </ButtonText>
          </li>
          <li>
            <span>Create your own tables</span>
          </li>
          {showSubscriptionLink && (
            <li>
              <Link to={`/subscriptions/new-subscription/${publication.slug}`}>
                Get email alerts
              </Link>
            </li>
          )}
        </ul>
      </nav>
    </div>
  );
};

export default ReleasePageTitleQuickLinks;
