import React from 'react';

const HomePageNav = ({
  links,
  title = 'On this page',
}: {
  links: { id: string; label: string }[];
  title?: string;
}) => {
  return (
    <nav
      className="govuk-!-margin-top-4"
      role="navigation"
      aria-labelledby="page-nav"
    >
      <h2 className="govuk-heading-s " id="page-nav">
        {title}
      </h2>
      {links.map(({ id, label }) => (
        <div
          key={id}
          className="govuk-!-display-inline-block govuk-!-margin-right-6"
        >
          <a href={`#${id}`} className="govuk-link--no-visited-state">
            {label}
          </a>
        </div>
      ))}
    </nav>
  );
};

export default HomePageNav;
