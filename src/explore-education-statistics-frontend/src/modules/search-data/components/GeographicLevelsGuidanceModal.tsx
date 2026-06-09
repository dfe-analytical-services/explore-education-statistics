import ButtonText from '@common/components/ButtonText';
import Modal from '@common/components/Modal';
import React from 'react';

export default function GeographicLevelsGuidanceModal() {
  return (
    <Modal
      showClose
      title="What are Geographic Levels?"
      triggerButton={<ButtonText>What are Geographic Levels?</ButtonText>}
    >
      <p>
        Geographic levels represent the different scales of administrative and
        physical boundaries used to group and report education data. Education
        policy and delivery happen at various scales (from individual schools to
        the national level), most data sets are broken down into these specific
        "geographies".
      </p>
      <h3>Why are Geographic Levels important?</h3>
      <p>
        When you search the data catalogue, the geographic level tells you how
        "local" or "broad" the data is. For example, some data sets only provide
        a high-level national overview, while others allow you to "drill down"
        into specific local councils or even individual schools.
      </p>

      <h3>Common Geographic Levels</h3>
      <p>
        Depending on the dataset, you will see one or more of the following
        levels:
      </p>

      <ul>
        <li>National (NAT): Data for the whole of England.</li>

        <li>
          Regional (REG): Data grouped into the nine official regions of England
          (e.g., North West, South East, London).
        </li>

        <li>
          Local Authority (LA): Data for the 150+ upper-tier local authorities
          responsible for education (e.g., Kent County Council, Manchester City
          Council).
        </li>

        <li>
          Local Authority District (LAD): A more granular breakdown of local
          government areas within a region.
        </li>

        <li>
          Parliamentary Constituency (PCON): Data based on the boundaries used
          for UK General Elections.
        </li>

        <li>
          School / Institution (INST/SCH): The most granular level, providing
          data for individual primary schools, secondary schools, or colleges.
        </li>

        <li>
          English Devolved Area (EDA) / Mayoral Combined Authority (MCA): Data
          for specific regions with devolved powers, such as Greater Manchester
          or the West Midlands.
        </li>
      </ul>
    </Modal>
  );
}
