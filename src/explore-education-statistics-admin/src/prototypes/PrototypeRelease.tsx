import PrototypePage from '@admin/prototypes/components/PrototypePage';
import React from 'react';
import classNames from 'classnames';
import styles from './PrototypePublicPage.module.scss';

const PrototypeRelease = () => {
  return (
    <div className={styles.prototypePublicPage}>
      <PrototypePage
        breadcrumbs={[
          { name: 'Find statistics and data', link: '#' },
          {
            name: 'Children looked after in England including adoptions',
            link: '#',
          },
        ]}
        title="Children looked after in England including adoptions"
        caption="Reporting year 2020"
        wide={false}
      >
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            <strong className="govuk-tag govuk-!-margin-right-6">
              This is the latest data
            </strong>
            <dl className="dfe-meta-content govuk-!-margin-top-3 govuk-!-margin-bottom-1">
              <dt className="govuk-caption-m">Published: </dt>
              <dd data-testid="published-date">
                <strong>
                  <time>10 December 2020</time>
                </strong>
              </dd>
              <div>
                <dt className="govuk-caption-m">Next update: </dt>
                <dd data-testid="next-update">
                  <strong>
                    <time>01 December 2021</time>
                  </strong>
                </dd>
              </div>
            </dl>
          </div>
          <div className="govuk-grid-column-one-third">1/3</div>
        </div>
        THIS IS A RELEASE TEST
      </PrototypePage>
    </div>
  );
};

export default PrototypeRelease;
