import React from 'react';
// import Details from '../components/Details';
import Link from '../components/Link';
import PrototypePage from './components/PrototypePage';
import PrototypeSearchForm from './components/PrototypeSearchForm';
// import PrototypeTileWithChart from './components/PrototypeTileWithChart';

const HomePage = () => {
  return (
    <PrototypePage>
      <h1 className="govuk-heading-xl">Explore education statistics</h1>
      <p className="govuk-body-l">
        Use this service to search for and find out about Department for
        Education (DfE) official statistics for England.
      </p>
      <h2 className="govuk-heading-l">What do you want to do?</h2>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-one-half">
          <h3 className="govuk-heading-m govuk-!-margin-bottom-0">
            <Link to="/prototypes/browse-releases">
              Browse and download statistical releases
            </Link>
          </h3>
          <p className="govuk-caption-m govuk-!-margin-top-2">
            Statistics from schools, higher education and social care, including
            absence and exclusions, capacity and admissions, results, teacher
            numbers
          </p>
        </div>
        <div className="govuk-grid-column-one-half">
          <h3 className="govuk-heading-m govuk-!-margin-bottom-0">
            <Link to="/prototypes/data-table-v1/national">
              Explore statistical data online
            </Link>
          </h3>
          <p className="govuk-caption-m govuk-!-margin-top-2">
            Data from further education, higher education and apprenticeships in
            England
          </p>
        </div>
      </div>
      <hr />
      <h2 className="govuk-heading-m govuk-!-margin-top-9">Other services</h2>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-one-half">
          <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
            <Link to="#">Get information about schools</Link>
          </h3>
          <p className="govuk-caption-m govuk-!-margin-top-0">
            Obcaecati minima distinctio porro nostrum. Dignissimos amet, sequi,
            pariatur odio dolor consequuntur ad omnis voluptatem unde, expedita
            facilis delectus fuga esse asperiores.
          </p>
        </div>
        <div className="govuk-grid-column-one-half">
          <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
            <Link to="#">Get information about schools</Link>
          </h3>
          <p className="govuk-caption-m govuk-!-margin-top-0">
            At dolore eligendi eaque molestias asperiores ullam exercitationem
            rerum inventore, cumque, quas eius voluptates
          </p>
        </div>
      </div>
    </PrototypePage>
  );
};

export default HomePage;
