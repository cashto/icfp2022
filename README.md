## Problem description

This year's task was to generate a series of drawing commands in order to create an image with highest possible similarity to a given picture, for a set of ~25 test pictures. Notably, drawing commands are limited to rectangle-shaped areas only, with a cost that varies inversely to the size of the rectangle (ie, smaller rectangles have higher cost).  Rectangle-shaped regions must be formed using a series of split and merge commands from the initial rectangular area which spans the entire canvas.

(There was also a "swap two regions" drawing command, which I assume was for entertainment purposes only. It was unclear whether there was any way to use this command productively; if there was, I was unable to find it).

## Approach

For the lightning division, solutions were generated manually.  I used the same [ASP.Net WebApi Core](https://docs.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-5.0) standalone server as was used in the previous two years, running on localhost, which hosts a webpage that displays the target image in an HTML canvas and allows the user to create a series of rectangle-shaped regions on top of it.  Upon clicking "submit", the localhost server would assign colors to each region (using the average color of the region), generate drawing commands, and send them to the contest server (I reverse-engineered the API from the contest portal before they had publicly documented it).

Rectangle-shaped regions were formed using line-cut commands which trimmed away the four sides in order of increasing size, so as to minimize the cost of forming the rectangular region.  Once the target rectangle was painted, the line-cut commands were undone in reverse order using merge commands.  I had some idea of switching to using point-cut commands, and potentially leaving regions unmerged if they were no longer used by the rest of the image, but analysis showed that this part contributed only a tiny fraction of the overall score, so I didn't optimize it further.

For the standard division, I used randomized search starting from a series of 49 rectangles arranged in a 7x7 grid.  Rectangles which do not contribute positively to the final score are removed before submission.

In the lightning division, images started from a blank, white canvas -- but in the standard division, an initial starting image was provided; and furthermore the initial area was already pre-divided into an array of between 100 and 400 blocks.  In theory, solvers could use this to their advantage, but mine did not -- instead I merged everything back to a single region and overpainted it with a fixed background color.  

Initially this remerge was by recursively merging 2x2 blocks into 1, but I realized this was more costly than the simpler option of merging blocks by row and then by column.

I did take the bait and spent a few hours exploring the use of swap commands from the initial background image, but it later became clear that it was mathematically impossible to get a decent score with this technique.

It turns out that although "average color" is a good approximation that minimizes the color distance to a set of pixels within a region, it isn't optimal. Unfortunately, there is no linear-time algorithm to minimize this loss function, so while the search algorithm used average color while searching, the actual colors chosen for submission are found by gradient descent, and thus were slightly different than the colors used during searching.

## Examples

![Target image](/images/input.png) ![Solving animation](/images/mona-lisa.gif)

Above shows 10,000 iterations of the solver, which takes about five minutes of execution time.

|Cost|Points|
|----|----|
|Similarity     |18,127|
|Painting       |3,390 |
|Region creation|2,211 |
|**Total**      |23,728|

The two images may not seem very similar to the human eye, which notices facial details and sharply-defined edges rather than "sum of color distances over the entire image".  But reduced to 1/16th size, the similarity becomes more readily apparent:

![Target image](/images/input-quarter.png) ![Solution image](/images/output-quarter.png) 

## Results

I was in 22nd place when the lightning division leaderboard froze, and 35th (of 150) when the standard division leaderboard froze.  While the leaderboard was frozen, I improved my score from 1,242,097 to 1,192,147, which would put me in 32nd place if no one else had made any improvement.

## Comments

The organizers for this year's contest were announced uncharacteristically late in the year.  I was apprehensive that as a result the contest would be rushed, buggy, unimaginative, or even perfectly-solvable as a result.  Fortunately none of these things turned out to be the case; this year's contest was run with the same level of quality as most other years.  Although I suspect the dominant approach for solving the problem was not exactly what the organizers had in mind, there were multiple avenues for solving the task and sufficient depth in each approach for there to be differentiation in the skills of the contest participants.
