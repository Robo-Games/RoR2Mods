# Uncapped Chances
Allows stacking chance items to roll multiple times past 100%. Supports crit, bleed collapse, ghor's tome and sticky bomb as these all stack chance linearly. Currently there are no plans on changing eulogy zero or lost seer's lenses as theres no real logical way to make them roll multiple times.

Provides option to toggle on or off each of the effects in the config file.

Each effect can additionally have a "Lower Successive" option turned on, meaning for each succesive roll of the effect, the chance is lowered. ie, for the second crit, crit chance is half as effective, for the third crit its 1/3rd as effective, and so on. By default this option is off.

There is an option for if crit damage is to stack up multiplicatively or additively, ie, 2>4>6 or 2>4>8.

Bleed additionally works a bit differently to vanilla, "bleed on hit" attacks and shatterspleen now no longer override the chance to bleed, each providing their own individual bleed stack seperate to tri-tip bleed chance.