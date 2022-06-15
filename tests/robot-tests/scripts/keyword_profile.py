#!/usr/bin/env python

#  License for this file only!
#  Source: http://bitbucket.org/robotframework/robottools/src/master/keywordtimes
#
#  Copyright 2014 Nokia Solutions and Networks
#
#  Licensed under the Apache License, Version 2.0 (the "License");
#  you may not use this file except in compliance with the License.
#  You may obtain a copy of the License at
#
#      http://www.apache.org/licenses/LICENSE-2.0
#
#  Unless required by applicable law or agreed to in writing, software
#  distributed under the License is distributed on an "AS IS" BASIS,
#  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
#  See the License for the specific language governing permissions and
#  limitations under the License.

"""
This is a tool that helps you to profile where the most of the time in your test cases is consumed.
This is helpful for example in situations where you want to optimise the test execution times.
"""

import sys

from robot.api import ExecutionResult

try:
    from robot.api import ResultVisitor
except ImportError:    # Not exposed via robot.api in RF 2.7
    from robot.result.visitor import ResultVisitor

import math
import re


class KeywordTimes(ResultVisitor):

    VAR_PATTERN = re.compile(r'^(\$|\@)\{[^\}]+\}(, \$\{[^\}]+\})* = ')

    def __init__(self):
        self.keywords = {}

    def end_keyword(self, keyword):
        name = self._get_name(keyword)
        if name not in self.keywords:
            self.keywords[name] = KeywordsTime(name)
        self.keywords[name].elapsedtimes += [keyword.elapsedtime]

    def _get_name(self, keyword):
        name = keyword.name
        m = self.VAR_PATTERN.search(name)
        if m:
            return name[m.end():]
        return name


class KeywordsTime(object):

    def __init__(self, name):
        self.name = name
        self.elapsedtimes = []

    @property
    def elapsed(self):
        return float(sum(self.elapsedtimes)) / 1000

    @property
    def calls(self):
        return len(self.elapsedtimes)

    @property
    def average_time(self):
        return round(float(self.elapsed) / self.calls, 3)

    @property
    def median_time(self):
        s = sorted(self.elapsedtimes)
        half = float(len(s) - 1) / 2
        half_low = int(math.floor(half))
        half_high = int(math.ceil(half))
        return round(float(s[half_low] + s[half_high]) / 2000, 3)

    @property
    def variance(self):
        squares = [(float(i) / 1000)**2 for i in self.elapsedtimes]
        return sum(squares) / len(squares) - (self.elapsed / self.calls)**2

    @property
    def standard_deviation(self):
        return round(self.variance**0.5, 3)

    @property
    def stdev_per_avgtime(self):
        if self.average_time == 0:
            return 0
        return round(100 * self.standard_deviation / self.average_time, 2)


def _print_results(times, shown_keywords, limit):
    ktlist = times.keywords.values()
    kwrows = []
    for kt in ktlist:
        kwrows.append([kt.elapsed, kt.calls, kt.average_time, kt.median_time,
                      kt.standard_deviation, kt.stdev_per_avgtime, kt.name])

    # NOTE(mark): Change x index to sort by a different column i.e. x[2] to sort by average_time
    kwrows.sort(key=lambda x: x[2], reverse=True)

    print('Total time (s) |   Calls | avg time (s) | median time (s) | stdev (s) | stdev/avg time % | Keyword name')
    shown = 0
    for k in kwrows:
        if shown >= shown_keywords:
            break
        if limit is not None and k.stdev_per_avgtime > limit:
            continue
        shown += 1
        print(str(k[0]).rjust(14) + ' | ' + str(k[1]).rjust(7) + ' | ' + str(k[2]).rjust(12) + ' | ' +
              str(k[3]).rjust(15) + ' | ' + str(k[4]).rjust(9) + ' | ' + str(k[5]).rjust(16) + ' | ' + str(k[6]))
    print('Showing %d of total keywords %d' % (shown, len(times.keywords)))


def _write_results(times, shown_keywords, limit, path):
    ktlist = times.keywords.values()
    kwrows = []
    for kt in ktlist:
        kwrows.append([kt.elapsed, kt.calls, kt.average_time, kt.median_time,
                      kt.standard_deviation, kt.stdev_per_avgtime, kt.name])

    # NOTE(mark): Change x index to sort by a different column i.e. x[2] to sort by average_time
    kwrows.sort(key=lambda x: x[2], reverse=True)

    fd = open(path, "w", encoding="utf-8")
    fd.write('Total time (s) |   Calls | avg time (s) | median time (s) | stdev (s) | stdev/avg time % | Keyword name\n')
    shown = 0
    for k in kwrows:
        if shown >= shown_keywords:
            break
        if limit is not None and k.stdev_per_avgtime > limit:
            continue
        shown += 1
        fd.write(str(k[0]).rjust(14) +
                 ' | ' +
                 str(k[1]).rjust(7) +
                 ' | ' +
                 str(k[2]).rjust(12) +
                 ' | ' +
                 str(k[3]).rjust(15) +
                 ' | ' +
                 str(k[4]).rjust(9) +
                 ' | ' +
                 str(k[5]).rjust(16) +
                 ' | ' +
                 str(k[6]) +
                 '\n')
    fd.write('Showing %d of total keywords %d\n' % (shown, len(times.keywords)))
    fd.close()


def run_keyword_profile(xmlfile, printresults=True, writepath='test-results/keyword-profiling-results.log'):
    try:
        resu = ExecutionResult(xmlfile)
        times = KeywordTimes()
        resu.visit(times)
        if printresults:
            _print_results(times, 100, None)
        else:
            _write_results(times, 100, None, writepath)
    except BaseException:
        print(__doc__)
        raise


if __name__ == '__main__':
    if len(sys.argv) <= 1:
        print("USAGE: python " + sys.argv[0] + " path/to/output.xml")
    else:
        run_keyword_profile(sys.argv[1])
