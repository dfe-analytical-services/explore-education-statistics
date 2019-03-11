import React from 'react';
// import Link from './Link';

const Breadcrumbs = () => {
  // const currentBreadcrumbIndex = breadcrumbs.length - 1;

  return (
    <div className="govuk-breadcrumbs">
      <ol className="govuk-breadcrumbs__list" data-testid="breadcrumbs--list">
        {/*{breadcrumbs.map((breadcrumb, index) =>*/}
        {/*index < currentBreadcrumbIndex ? (*/}
        {/*<li key={index} className="govuk-breadcrumbs__list-item">*/}
        {/*<Link*/}
        {/*className="govuk-breadcrumbs__link"*/}
        {/*to={breadcrumb.props.match.url}*/}
        {/*>*/}
        {/*{breadcrumb}*/}
        {/*</Link>*/}
        {/*</li>*/}
        {/*) : (*/}
        {/*<li*/}
        {/*key={index}*/}
        {/*className="govuk-breadcrumbs__list-item"*/}
        {/*aria-current="page"*/}
        {/*>*/}
        {/*{breadcrumb}*/}
        {/*</li>*/}
        {/*),*/}
        {/*)}*/}
      </ol>
    </div>
  );
};

export default Breadcrumbs;
