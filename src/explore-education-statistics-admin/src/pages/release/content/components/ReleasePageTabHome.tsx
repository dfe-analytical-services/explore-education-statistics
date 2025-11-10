import ReleasePageTabPanel from '@admin/pages/release/content/components/ReleasePageTabPanel';
import { useReleaseContentState } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import ReleasePageLayout from '@common/modules/release/components/ReleasePageLayout';
import React from 'react';

interface Props {
  hidden: boolean;
}

const ReleasePageTabHome = ({ hidden }: Props) => {
  const { release } = useReleaseContentState();

  return (
    <ReleasePageTabPanel tabKey="home" hidden={hidden}>
      <ReleasePageLayout navItems={[]}>
        <h2>{release.title}</h2>
        <p>
          Per melius aperiri eu. Et interesset philosophia vim, graece denique
          intellegam duo at, te vix quot apeirian dignissim. Ei essent
          percipitur nam, natum possit interpretaris sea ea. Cum assum adipisci
          cotidieque ut, ut veri tollit duo. Erat idque volutpat mea ut, mel
          nominati splendide vulputate ea. No ferri partem ceteros pro. Everti
          volumus menandri at pro. Cum illud euripidis cu, mazim deterruisset ei
          eum. Ex alia dolorem insolens per, malis clita laboramus duo ut,
          ridens appareat philosophia ea quo.
        </p>
        <p>
          Per melius aperiri eu. Et interesset philosophia vim, graece denique
          intellegam duo at, te vix quot apeirian dignissim. Ei essent
          percipitur nam, natum possit interpretaris sea ea. Cum assum adipisci
          cotidieque ut, ut veri tollit duo. Erat idque volutpat mea ut, mel
          nominati splendide vulputate ea. No ferri partem ceteros pro. Everti
          volumus menandri at pro. Cum illud euripidis cu, mazim deterruisset ei
          eum. Ex alia dolorem insolens per, malis clita laboramus duo ut,
          ridens appareat philosophia ea quo.
        </p>
      </ReleasePageLayout>
    </ReleasePageTabPanel>
  );
};

export default ReleasePageTabHome;
