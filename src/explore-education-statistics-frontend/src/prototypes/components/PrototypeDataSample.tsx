import React from 'react';
import PrototypeChartSample from './PrototypeChartSample';
import PrototypeTableSample from './PrototypeTableSample';

interface Props {
  sectionId?: string;
  chartTitle?: string;
}

const PrototypeDataSample = ({ sectionId, chartTitle }: Props) => {
  return (
    <>
      <div className="govuk-tabs" data-module="tabs">
        <ul className="govuk-tabs__list">
          <li className="govuk-tabs__list-item">
            <a
              className="govuk-tabs__tab govuk-tabs__tab--selected"
              href={`#${sectionId}ChartData`}
            >
              Summary
            </a>
          </li>
          <li className="govuk-tabs__list-item">
            <a className="govuk-tabs__tab" href={`#${sectionId}TableData`}>
              Data tables
            </a>
          </li>
          <li className="govuk-tabs__list-item">
            <a className="govuk-tabs__tab" href={`#${sectionId}Downloads`}>
              Data downloads
            </a>
          </li>
        </ul>
        <section className="govuk-tabs__panel" id={`${sectionId}ChartData`}>
          <h2 className="govuk-heading-s">{`Chart showing ${chartTitle}`}</h2>
          <PrototypeChartSample />
        </section>
        <section
          className="govuk-tabs__panel govuk-tabs__panel--hidden"
          id={`${sectionId}TableData`}
        >
          <h2 className="govuk-heading-s">{`Table showing ${chartTitle}`}</h2>
          <PrototypeTableSample />
        </section>
      </div>
    </>
  );
};

export default PrototypeDataSample;
