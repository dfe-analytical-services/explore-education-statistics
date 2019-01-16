import React from 'react';
import Tabs from '../../components/Tabs';
import TabsSection from '../../components/TabsSection';
import PrototypeChartSample from './PrototypeChartSample';
import PrototypeTableSample from './PrototypeTableSample';

interface Props {
  sectionId?: string;
  chartTitle?: string;
}

const PrototypeDataSample = ({ sectionId, chartTitle }: Props) => {
  return (
    <>
      <Tabs>
        <TabsSection id={`${sectionId}ChartData`} title="Summary">
          <h2 className="govuk-heading-s">{`Chart showing ${chartTitle}`}</h2>
          <PrototypeChartSample />
        </TabsSection>
        <TabsSection id={`${sectionId}TableData`} title="Data tables">
          <h2 className="govuk-heading-s">{`Table showing ${chartTitle}`}</h2>
          <PrototypeTableSample />
        </TabsSection>
        <TabsSection id={`${sectionId}Downloads`} title="Data downloads">
          <h2 className="govuk-heading-s">
            Download overall absence data files
          </h2>
          <ul className="govuk-list">
            <li>
              <a className="govuk-link" href="#">
                Excel table
              </a>
            </li>
            <li>
              <a className="govuk-link" href="#">
                csv
              </a>
            </li>
            <li>
              <a className="govuk-link" href="#">
                json
              </a>
            </li>
            <li>
              <a className="govuk-link" href="#">
                API
              </a>
            </li>
          </ul>
        </TabsSection>
      </Tabs>
    </>
  );
};

export default PrototypeDataSample;
