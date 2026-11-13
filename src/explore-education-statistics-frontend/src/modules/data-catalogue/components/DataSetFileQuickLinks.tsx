import ButtonText from '@common/components/ButtonText';
import styles from '@common/modules/release/components/ReleasePageQuickLinks.module.scss';
import Link from '@frontend/components/Link';

interface Props {
  handleDownload: () => Promise<void>;
  subscribeFileId?: string;
  tableToolLink: string;
}

const DataSetFileQuickLinks = ({
  handleDownload,
  subscribeFileId,
  tableToolLink,
}: Props) => {
  const showSubscriptionLink = !!subscribeFileId;

  return (
    <div className={styles.quickLinksContainer}>
      <h2 className={styles.quickLinksHeading} id="quick-links">
        Quick links
      </h2>
      <nav
        role="navigation"
        aria-labelledby="quick-links"
        data-testid="quick-links"
      >
        <ul
          className={`${styles.quickLinks} govuk-list govuk-list--spaced govuk-!-margin-bottom-0`}
        >
          <li>
            <ButtonText onClick={handleDownload}>
              Download data set (ZIP)
            </ButtonText>
          </li>
          <li
            className={
              showSubscriptionLink ? undefined : 'govuk-!-margin-bottom-0'
            }
          >
            <Link to={tableToolLink}>Create your own tables</Link>
          </li>
          {showSubscriptionLink && (
            <li>
              <Link
                to={`/api-subscriptions/new-subscription/${subscribeFileId}`}
              >
                Get email alerts
              </Link>
            </li>
          )}
        </ul>
      </nav>
    </div>
  );
};

export default DataSetFileQuickLinks;
