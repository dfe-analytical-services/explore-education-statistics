type PreReleaseAccess = 'Before' | 'After' | 'Within' | 'NoneSet';

interface PreReleaseWindowStatus {
  preReleaseAccess: PreReleaseAccess;
  preReleaseWindowStartTime: Date;
  preReleaseWindowEndTime: Date;
}
