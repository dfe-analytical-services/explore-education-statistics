import React from 'react';

import styles from '@frontend/modules/home/components/HomePageNav.module.scss';

const HomePageNav = ({
  links,
  title = 'On this page',
}: {
  links: { id: string; label: string }[];
  title?: string;
}) => {
  return (
    <nav className="govuk-!-margin-top-4" aria-labelledby="page-nav">
      <h2 className="govuk-heading-s " id="page-nav">
        {title}
      </h2>
      <div className={styles.navigationContainer}>
        {links.map(({ id, label }) => (
          <div key={id} className="govuk-!-display-inline-block">
            <a href={`#${id}`} className="govuk-link--no-visited-state">
              {label}
            </a>
          </div>
        ))}
      </div>
    </nav>
  );
};

export default HomePageNav;
