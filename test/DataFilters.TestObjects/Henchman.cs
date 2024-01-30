namespace DataFilters.TestObjects;

#if NET
public class Henchman : SuperHero;
#else
public class Henchman : SuperHero
{
} 
#endif
