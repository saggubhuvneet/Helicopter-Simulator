using UnityEngine;
using MathNet.Numerics;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Oyedoyin.Rotary
{

    [System.Serializable]
    public struct cfloat { public float R; public float i; }
    public class MathR
    {
        /// <summary>
        /// Conversion variables
        /// </summary>
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <value>Converts inertia from slug/ft2 to kg/m2.</value>
        public const float SLGFT = 1.35582F;
        /// <value>Gets the horizontal speed conversion float from m/s to knots.</value>
        public const float toKnots = 1.94384F;
        /// <value>Gets the conversion float from m to ft.</value>
        public const float toFt = 3.28084F;
        /// <value>Gets the vertical speed conversion float from m/s to ft/min.</value>
        public const float toFtMin = 196.8504F;
        /// <value>Converts weight from pounds to kg.</value>
        public const float LBSKG = 2.205F;



        public static float Lim(float maximum, float minimum, float value) { if (value > maximum) return maximum; if (value < minimum) return minimum; return value; }
        public static float Square(float value) { return value * value; }

        public static float EstimateEffectiveValue(float rootValue, float tipValue, float r) { float baseValue = rootValue + (r * (tipValue - rootValue)); return baseValue; }
        public class FoilDesign
        {
            /// <summary>
            /// Does all the "Unclean" stuff for the aerofoils :))
            /// </summary>
#if UNITY_EDITOR

            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            public static void ShapeAerofoil(PhantomAerofoil foil, bool selected)
            {
                // ------------------------ Collider Check
                foil.foilCollider = foil.gameObject.GetComponent<BoxCollider>();
                if (foil.foilCollider == null) { foil.foilCollider = foil.gameObject.AddComponent<BoxCollider>(); }
                if (foil.foilCollider != null) { foil.foilCollider.size = new Vector3(1.0f, 0.1f, 1.0f); }


                // ------------------------ Draw Airfoils
                if (foil.drawFoils)
                {
                    if (foil.rootAirfoil != null) { PlotAirfoil(foil.RootChordLeading, foil.RootChordTrailing, foil.rootAirfoil, foil.transform, out foil.rootAirfoilArea, out foil.rootPoints); }
                    if (foil.tipAirfoil != null) { PlotAirfoil(foil.TipChordLeading, foil.TipChordTrailing, foil.tipAirfoil, foil.transform, out foil.tipAirfoilArea, out foil.tipPoints); }
                }

                if (selected || (!selected && !Application.isPlaying))
                {
                    // ------------------------ Connection Points
                    Vector3 adjustedTipCenter = MathBase.EstimateSectionPosition(foil.TipChordLeading, foil.TipChordTrailing, 0.5f);
                    Handles.DrawDottedLine(foil.rootChordCenter, adjustedTipCenter, 4f);
                    Handles.color = Color.yellow; Handles.DrawDottedLine(foil.quaterRootChordPoint, foil.quaterTipChordPoint, 4f);
                    if (foil.rootAirfoil == null || foil.tipAirfoil == null || !foil.drawFoils)
                    {
                        Gizmos.color = Color.yellow; Gizmos.DrawLine(foil.RootChordLeading, foil.RootChordTrailing);
                        Gizmos.color = Color.yellow; Gizmos.DrawLine(foil.TipChordTrailing, foil.TipChordLeading);
                    }


                    // ------------------------ Connection Points
                    Handles.DrawDottedLine(foil.rootChordCenter, adjustedTipCenter, 4f);
                    Handles.color = Color.yellow; Handles.DrawDottedLine(foil.quaterRootChordPoint, foil.quaterTipChordPoint, 4f);

                    Gizmos.color = Color.yellow; Gizmos.DrawLine(foil.RootChordLeading, foil.TipChordLeading); //LEADING SPAN
                    Gizmos.color = Color.red; Gizmos.DrawLine(foil.TipChordTrailing, foil.RootChordTrailing); //TRAILING SPAN
                    Handles.color = Color.yellow; Handles.ArrowHandleCap(0, foil.TipChordLeading, foil.transform.rotation * Quaternion.LookRotation(Vector3.up), 0.3f, EventType.Repaint);

                }

                if (selected)
                {
                    // ------------------------ Extrapolate Panels
                    for (int p = 1; p < foil.foilSubdivisions; p++)
                    {
                        // ----------------- Division Factors
                        float currentSection = (float)p, nextSection = (float)(p + 1); float sectionLength = (float)foil.foilSubdivisions; float sectionFactor = currentSection / sectionLength;
                        Vector3 LeadingPointA, TrailingPointA;

                        // ---------------- Points
                        TrailingPointA = MathBase.EstimateSectionPosition(foil.RootChordTrailing, foil.TipChordTrailing, sectionFactor); LeadingPointA = MathBase.EstimateSectionPosition(foil.RootChordLeading, foil.TipChordLeading, sectionFactor);

                        // ---------------- Mark
                        Gizmos.color = Color.yellow; Gizmos.DrawLine(LeadingPointA, TrailingPointA);
                        float yM = Vector3.Distance(foil.RootChordTrailing, TrailingPointA);
                        if (foil.drawFoils)
                        {
                            if (foil.rootAirfoil != null && foil.tipAirfoil != null) { PlotRibAirfoil(LeadingPointA, TrailingPointA, yM, 0, Color.yellow, false, foil.rootAirfoil, foil.tipAirfoil, foil.foilSpan, foil.transform); }
                        }
                    }
                }
            }
#endif

            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            public static void PlotAirfoil(Vector3 leadingPoint, Vector3 trailingPoint, SilantroAirfoil foil, Transform foilTransform, out float foilArea, out List<Vector3> points)
            {
                points = new List<Vector3>(); List<float> xt = new List<float>(); float chordDistance = Vector3.Distance(leadingPoint, trailingPoint);
                Vector3 PointA = Vector3.zero, PointXA = Vector3.zero, PointXB = Vector3.zero, PointB = Vector3.zero;

                //FIND POINTS
                if (foil.x.Count > 0)
                {
                    for (int j = 0; (j < foil.x.Count); j++)
                    {
                        //BASE POINT
                        Vector3 XA = leadingPoint - ((leadingPoint - trailingPoint).normalized * (foil.x[j] * chordDistance)); Vector3 liftDirection = foilTransform.up.normalized;
                        PointA = XA + (liftDirection * ((foil.y[j]) * chordDistance)); points.Add(PointA); if ((j + 1) < foil.x.Count) { Vector3 XB = (leadingPoint - ((leadingPoint - trailingPoint).normalized * (foil.x[j + 1] * chordDistance))); PointB = XB + (liftDirection.normalized * ((foil.y[j + 1]) * chordDistance)); }
                        //CONNECT
                        Gizmos.color = Color.white; Gizmos.DrawLine(PointA, PointB);
                    }
                }

                //PERFORM CALCULATIONS
                xt = new List<float>();
                for (int j = 0; (j < points.Count); j++) { xt.Add(Vector3.Distance(points[j], points[(points.Count - j - 1)])); Gizmos.DrawLine(points[j], points[(points.Count - j - 1)]); }
                foilArea = Mathf.Pow(chordDistance, 2f) * (((foil.xtc * 0.01f) + 3) / 6f) * (foil.tc * 0.01f);
            }


            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            public static void PlotRibAirfoil(Vector3 leadingPoint, Vector3 trailingPoint, float distance, float wingTip, Color ribColor, bool drawSplits, SilantroAirfoil rootAirfoil, SilantroAirfoil tipAirfoil, float span, Transform foilTransform)
            {
                List<Vector3> points = new List<Vector3>(); List<float> xt = new List<float>(); float chordDistance = Vector3.Distance(leadingPoint, trailingPoint);
                Vector3 PointA = Vector3.zero, PointXA = Vector3.zero, PointXB = Vector3.zero;
                //FIND POINTS
                if (rootAirfoil.x.Count > 0)
                {
                    for (int j = 0; (j < rootAirfoil.x.Count); j++)
                    {
                        float xi = MathBase.EstimateEffectiveValue(rootAirfoil.x[j], tipAirfoil.x[j], distance, wingTip, span); float yi = MathBase.EstimateEffectiveValue(rootAirfoil.y[j], tipAirfoil.y[j], distance, wingTip, span);
                        //BASE POINT
                        Vector3 XA = leadingPoint - ((leadingPoint - trailingPoint).normalized * (xi * chordDistance)); Vector3 liftDirection = foilTransform.up; PointXA = XA + (liftDirection * yi * chordDistance); points.Add(PointXA);
                        if ((j + 1) < rootAirfoil.x.Count)
                        {
                            float xii = MathBase.EstimateEffectiveValue(rootAirfoil.x[j + 1], tipAirfoil.x[j + 1], distance, wingTip, span); float yii = MathBase.EstimateEffectiveValue(rootAirfoil.y[j + 1], tipAirfoil.y[j + 1], distance, wingTip, span);
                            Vector3 XB = (leadingPoint - ((leadingPoint - trailingPoint).normalized * (xii * chordDistance))); PointXB = XB + (liftDirection.normalized * (yii * chordDistance));
                        }
                        //CONNECT
                        Gizmos.color = ribColor; Gizmos.DrawLine(PointXA, PointXB);
                    }
                }
            }



            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            public static void MapSurface(PhantomAerofoil foil)
            {
                if (foil.finType == PhantomAerofoil.FinType.Rudder) { foil.controlColor = Color.red; }
                if (foil.stabType == PhantomAerofoil.StabilatorType.Elevon) { foil.controlColor = new Color(0, 0, 0.5f); }
                if (foil.stabType == PhantomAerofoil.StabilatorType.Elevator) { foil.controlColor = Color.blue; }
                if (foil.controlSections == null || foil.foilSubdivisions != foil.controlSections.Length) { foil.controlSections = new bool[foil.foilSubdivisions]; }
                if (foil.stabType == PhantomAerofoil.StabilatorType.Elevator) { foil.controlTipChord = 100f; foil.controlRootChord = 100f; for (int a = 0; a < foil.controlSections.Length; a++) { foil.controlSections[a] = true; } }
                if (foil.finType == PhantomAerofoil.FinType.Rudder || foil.stabType == PhantomAerofoil.StabilatorType.Elevator || foil.stabType == PhantomAerofoil.StabilatorType.Elevon)
                {
                    EstimateControlSurface(foil, foil.controlTipChord, foil.controlRootChord, foil.controlSections, foil.controlColor, false, false, foil.controlDeflection, true, out foil.controlSpan, out foil.controlArea);
                }
            }


            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            public static void EstimateControlSurface(PhantomAerofoil foil, float inputTipChord, float inputRootChord, bool[] sections, Color surfaceColor, bool leading, bool floating, float surfaceDeflection, bool drawSections, out float surfaceSpan, out float surfaceArea)
            {
                //COLLECTION
                surfaceArea = 0; surfaceSpan = 0f;

                if (sections != null)
                {
                    for (int i = 0; i < sections.Length; i++)
                    {
                        if (sections[i] == true)
                        {
                            //BUILD VARIABLES
                            float currentSelection = (float)i; float nextSection = (float)(i + 1); float sectionLength = (float)sections.Length;
                            float baseFactorA = currentSelection / sectionLength; float baseFactorB = nextSection / sectionLength;
                            //DRAW CONTROL SURFACE
                            Vector3[] rects = new Vector3[4];

                            rects[0] = (MathBase.EstimateSectionPosition(MathBase.EstimateSectionPosition(foil.RootChordTrailing, foil.RootChordLeading, (inputRootChord * 0.01f)),
                            MathBase.EstimateSectionPosition(foil.TipChordTrailing, foil.TipChordLeading, (inputTipChord * 0.01f)), baseFactorA));
                            rects[1] = (MathBase.EstimateSectionPosition(foil.RootChordTrailing, foil.TipChordTrailing, baseFactorA));
                            rects[2] = (MathBase.EstimateSectionPosition(foil.RootChordTrailing, foil.TipChordTrailing, baseFactorB));
                            rects[3] = (MathBase.EstimateSectionPosition(MathBase.EstimateSectionPosition(foil.RootChordTrailing, foil.RootChordLeading, (inputRootChord * 0.01f)),
                            MathBase.EstimateSectionPosition(foil.TipChordTrailing, foil.TipChordLeading, (inputTipChord * 0.01f)), baseFactorB));
                            //DEFLECT SURFACE
                            rects[1] = rects[0] + Quaternion.AngleAxis(surfaceDeflection, (rects[3] - rects[0]).normalized) * (rects[1] - rects[0]);
                            rects[2] = rects[3] + (Quaternion.AngleAxis(surfaceDeflection, (rects[3] - rects[0]).normalized)) * (rects[2] - rects[3]);

#if UNITY_EDITOR
                            //DRAW CONTROL AIRFOILS
                            if (drawSections && (foil.rootAirfoil != null && foil.tipAirfoil != null))
                            {
                                float yM = Vector3.Distance(foil.RootChordTrailing, rects[1]);
                                if (foil.drawFoils) { if ((foil.rootAirfoil != null && foil.tipAirfoil != null)) { PlotRibAirfoil(rects[0], rects[1], yM, 0, Color.white, false, foil.rootAirfoil, foil.tipAirfoil, foil.foilSpan, foil.transform); } }
                            }
                            //DRAW CONTROLS
                            Handles.color = surfaceColor;
                            Handles.DrawSolidRectangleWithOutline(rects, surfaceColor, surfaceColor);
#endif
                            surfaceArea += MathBase.EstimatePanelSectionArea(rects[0], rects[3], rects[1], rects[2]);
                            Vector3 rootCenter = MathBase.EstimateSectionPosition(rects[0], rects[1], 0.5f); Vector3 tipCenter = MathBase.EstimateSectionPosition(rects[2], rects[3], 0.5f);
                            float panelSpan = Vector3.Distance(rootCenter, tipCenter); surfaceSpan += panelSpan;
                        }
                    }
                }
            }
        }


















        public class RotorDesign
        {

            /// <summary>
            /// Handles the rotor design and debug draw
            /// </summary>
#if UNITY_EDITOR
            public static void AnalyseRotorShape(PhantomRotor rotor)
            {
                float ψ = 360 / rotor.Nb;
                rotor.skewDistance = rotor.transform.up * rotor.rotorRadius * Mathf.Sin(rotor.coneAngle * Mathf.Deg2Rad);
                if (rotor.twistType == PhantomRotor.TwistType.Upward) { rotor.ϴtw = rotor.bladeWashout; }
                else if (rotor.twistType == PhantomRotor.TwistType.Constant) { rotor.ϴtw = 0f; }
                else { rotor.ϴtw = -rotor.bladeWashout; }
                rotor.rootcut = (rotor.rotorRadius * rotor.rootCutOut) - rotor.rotorHeadRadius;
                rotor.bladeRadius = ((1 - rotor.rootCutOut) * rotor.rotorRadius) + rotor.rootcut;
                rotor.hingeOffset = rotor.re * rotor.bladeRadius;
                rotor.J = rotor.Nb * rotor.bladeMass * Mathf.Pow((rotor.bladeRadius * 0.55f), 2);
                float m = rotor.bladeMass / rotor.bladeRadius;
                rotor.Iβ = ((m * Mathf.Pow(rotor.bladeRadius, 3)) / 3) * Mathf.Pow((1 - rotor.re), 3);
                rotor.aspectRatio = ((rotor.bladeRadius * rotor.bladeRadius) / (rotor.bladeRadius * rotor.bladeChord));
                if (rotor.weightUnit == PhantomRotor.WeightUnit.Kilogram) { rotor.weightFactor = 1f; }
                if (rotor.weightUnit == PhantomRotor.WeightUnit.Pounds) { rotor.weightFactor = (1 / 2.205f); }
                rotor.actualWeight = rotor.bladeMass * rotor.weightFactor;

                for (int i = 0; i < rotor.Nb; i++)
                {
                    float currentSector = ψ * (i + 1);
                    Quaternion sectorRotation = Quaternion.AngleAxis(currentSector, rotor.transform.up);
                    Vector3 sectorTipPosition = rotor.transform.position + (sectorRotation * (rotor.transform.forward * rotor.rotorRadius));
                    Vector3 hingePosition = rotor.transform.position + (sectorRotation * (rotor.transform.forward * rotor.hingeOffset));
                    sectorTipPosition += rotor.skewDistance;

                    // ---------------------------------- Base Factors
                    Vector3 bladeForward = sectorTipPosition - rotor.transform.position;
                    Vector3 bladeRight = Vector3.Cross(bladeForward.normalized, rotor.transform.up.normalized);
                    Vector3 bladeRootCenter = rotor.transform.position + (bladeForward * rotor.rootCutOut) + (bladeRight * rotor.rootDeviation);
                    var supremeFactor = bladeRight * rotor.bladeChord * 0.5f;
                    float featherAngle = rotor.ϴ0 * Mathf.Rad2Deg; if (rotor.rotorDirection == PhantomRotor.RotationDirection.CW) { featherAngle *= -1; }
                    sectorTipPosition += (bladeRight * rotor.rootDeviation);
                    // ------------------------------------ Structure Points
                    if (rotor.rotorDirection == PhantomRotor.RotationDirection.CCW)
                    {
                        rotor.tipLeadingEdge = sectorTipPosition + supremeFactor; rotor.rootLeadingEdge = bladeRootCenter + supremeFactor;
                        rotor.tipTrailingEdge = sectorTipPosition - supremeFactor; rotor.rootTrailingEdge = bladeRootCenter - supremeFactor;
                    }
                    else
                    {
                        rotor.tipLeadingEdge = sectorTipPosition - supremeFactor; rotor.rootLeadingEdge = bladeRootCenter - supremeFactor;
                        rotor.tipTrailingEdge = sectorTipPosition + supremeFactor; rotor.rootTrailingEdge = bladeRootCenter + supremeFactor;
                    }


                    Gizmos.color = Color.cyan;
                    Gizmos.DrawSphere(hingePosition, 0.05f);
                    Gizmos.color = Color.cyan; Gizmos.DrawLine(hingePosition, hingePosition + (rotor.transform.up * 1f));


                    // ---------------------------------- Blade Feathering
                    rotor.rootDeflection = -featherAngle + rotor.ϴtw; if (rotor.rotorDirection == PhantomRotor.RotationDirection.CCW) { rotor.rootDeflection = -featherAngle - rotor.ϴtw; }
                    Vector3 rootSkew = Quaternion.AngleAxis(-featherAngle, bladeForward) * (rotor.tipLeadingEdge - rotor.tipTrailingEdge) * 0.5f;
                    Vector3 tipSkew = Quaternion.AngleAxis(rotor.rootDeflection, bladeForward) * (rotor.tipLeadingEdge - rotor.tipTrailingEdge) * 0.5f;
                    rotor.tipTrailingEdge = sectorTipPosition - tipSkew; rotor.tipLeadingEdge = sectorTipPosition + tipSkew;
                    rotor.rootTrailingEdge = bladeRootCenter - rootSkew; rotor.rootLeadingEdge = bladeRootCenter + rootSkew;
                    rotor.quaterRootChordPoint = MathBase.EstimateSectionPosition(rotor.rootLeadingEdge, rotor.rootTrailingEdge, 0.25f);
                    rotor.quaterTipChordPoint = MathBase.EstimateSectionPosition(rotor.tipLeadingEdge, rotor.tipTrailingEdge, 0.25f);




                    //DRAW DISC
                    Handles.color = Color.red;
                    Handles.DrawWireDisc((rotor.transform.position + rotor.skewDistance), rotor.transform.up, (rotor.rotorRadius));
                    Handles.color = Color.cyan;
                    Handles.DrawWireDisc(rotor.transform.position, rotor.transform.up, (rotor.rotorHeadRadius));

                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(rotor.tipTrailingEdge, rotor.rootTrailingEdge);
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(rotor.tipLeadingEdge, rotor.rootLeadingEdge);
                    Gizmos.DrawLine(rotor.transform.position, rotor.rootLeadingEdge); Gizmos.DrawLine(rotor.transform.position, rotor.rootTrailingEdge);

                    if (rotor.rotorConfiguration != PhantomRotor.RotorConfiguration.Propeller)
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawSphere(rotor.transform.position, 0.1f);
                        Gizmos.color = Color.green; Gizmos.DrawLine(rotor.transform.position, rotor.transform.position + (rotor.transform.up * 2f));
                    }

                    if (rotor.drawFoils && rotor.rootAirfoil != null && rotor.tipAirfoil != null)
                    {
                        float ta; List<Vector3> tb;
                        FoilDesign.PlotAirfoil(rotor.rootLeadingEdge, rotor.rootTrailingEdge, rotor.rootAirfoil, rotor.transform, out ta, out tb);
                        FoilDesign.PlotAirfoil(rotor.tipLeadingEdge, rotor.tipTrailingEdge, rotor.tipAirfoil, rotor.transform, out ta, out tb);


                        // ---------------------------------- Draw Subdividions
                        if (rotor.drawFoils && rotor.rootAirfoil != null && rotor.tipAirfoil != null)
                        {
                            for (int p = 1; p < rotor.bladeSubdivisions; p++)
                            {
                                float currentSection = p; float sectionLength = (float)rotor.bladeSubdivisions; float sectionFactor = currentSection / sectionLength;
                                Vector3 LeadingPointA, TrailingPointA;
                                TrailingPointA = MathBase.EstimateSectionPosition(rotor.rootTrailingEdge, rotor.tipTrailingEdge, sectionFactor);
                                LeadingPointA = MathBase.EstimateSectionPosition(rotor.rootLeadingEdge, rotor.tipLeadingEdge, sectionFactor);
                                Gizmos.color = Color.yellow; Gizmos.DrawLine(LeadingPointA, TrailingPointA);
                                float yM = Vector3.Distance(rotor.rootTrailingEdge, TrailingPointA);
                                FoilDesign.PlotRibAirfoil(LeadingPointA, TrailingPointA, yM, 0, Color.yellow, false, rotor.rootAirfoil, rotor.tipAirfoil, rotor.rotorRadius, rotor.transform);
                            }
                        }
                    }
                    else
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawLine(rotor.rootTrailingEdge, rotor.rootLeadingEdge);
                        Gizmos.DrawLine(rotor.tipLeadingEdge, rotor.tipTrailingEdge);
                    }


                    // ---------------------------------- Blade Helpers
                    Handles.color = Color.yellow; Handles.DrawDottedLine(rotor.quaterRootChordPoint, rotor.quaterTipChordPoint, 4f);
                    Vector3 tipDirection = (rotor.tipLeadingEdge - rotor.tipTrailingEdge); Quaternion tipRotation = Quaternion.LookRotation(tipDirection, rotor.transform.up);
                    Handles.color = Color.green; Handles.ArrowHandleCap(0, rotor.tipLeadingEdge, rotor.transform.rotation * Quaternion.LookRotation(Vector3.up), 0.3f, EventType.Repaint);

                    if(rotor.rotorConfiguration == PhantomRotor.RotorConfiguration.Propeller)
                    {
                        Handles.color = Color.yellow; Handles.ArrowHandleCap(0, rotor.tipLeadingEdge, tipRotation, 0.3f, EventType.Repaint);
                        Vector3 rootDirection = (rotor.rootLeadingEdge - rotor.rootTrailingEdge); Quaternion rootRotation = Quaternion.LookRotation(rootDirection, rotor.transform.up);
                        Handles.color = Color.red; Handles.ArrowHandleCap(0, rotor.rootLeadingEdge, rootRotation, 0.3f, EventType.Repaint);
                    }
                    else
                    {
                        Handles.color = Color.yellow; Handles.ArrowHandleCap(0, rotor.tipLeadingEdge, tipRotation, 0.8f, EventType.Repaint);
                        Vector3 rootDirection = (rotor.rootLeadingEdge - rotor.rootTrailingEdge); Quaternion rootRotation = Quaternion.LookRotation(rootDirection, rotor.transform.up);
                        Handles.color = Color.red; Handles.ArrowHandleCap(0, rotor.rootLeadingEdge, rootRotation, 0.8f, EventType.Repaint);

                    }
                }
            }
#endif
        }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static Complex32 SwirlFactor(Complex32 CT, Complex32 x)
        {
            Complex32 swirl = ((Complex32.Pow((1 - Complex32.Sqrt(1 - ((2 * CT) / (x * x)))), 2) * Complex32.Pow(x, 3)) / CT);
            return swirl;
        }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static Quaternion Damp(Quaternion a, Quaternion b, float lambda, float dt)
        {
            return Quaternion.Slerp(a, b, 1 - Mathf.Exp(-lambda * dt));
        }

        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void EstimateControlExtension(Vector3 LeadEdgeLeft, Vector3 TrailEdgeRight, Vector3 TrailEdgeLeft, Vector3 LeadEdgeRight, float inputRootChord, float inputTipChord, float deflection, out Vector3 leftControlExtension, out Vector3 rightControlExtension)
        {
            leftControlExtension = (Quaternion.AngleAxis(deflection, ((MathBase.EstimateSectionPosition(TrailEdgeRight, LeadEdgeRight, (inputTipChord * 0.01f)) -
            MathBase.EstimateSectionPosition(TrailEdgeLeft, LeadEdgeLeft, (inputRootChord * 0.01f))).normalized))) * (TrailEdgeLeft - MathBase.EstimateSectionPosition(TrailEdgeLeft, LeadEdgeLeft, (inputRootChord * 0.01f)));
            rightControlExtension = (Quaternion.AngleAxis(deflection, ((MathBase.EstimateSectionPosition(TrailEdgeRight, LeadEdgeRight, (inputTipChord * 0.01f)) -
            MathBase.EstimateSectionPosition(TrailEdgeLeft, LeadEdgeLeft, (inputRootChord * 0.01f))).normalized))) * (TrailEdgeRight - MathBase.EstimateSectionPosition(TrailEdgeRight, LeadEdgeRight, (inputTipChord * 0.01f)));
        }



        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static float CDf(float αd, float Mr, Complex32 alpha)
        {
            Complex32 cd = 0f; Complex32 cdi;
            if (alpha.Real > αd)
            {
                if (Mr < 0.1f) { cd = 0.0081f + (((-350 * alpha) + (396 * Complex32.Pow(alpha, 2)) - (63.3f * Complex32.Pow(alpha, 3)) + (3.66f * Complex32.Pow(alpha, 4))) * Mathf.Pow(10, (-6))); }
                if (Mr > 0.1f && Mr < 0.725f) { cdi = 0.0081f + (((-350 * alpha) + (396 * Complex32.Pow(alpha, 2)) - (63.3f * Complex32.Pow(alpha, 3)) + (3.66f * (Complex32.Pow(alpha, 4)))) * Mathf.Pow(10, (-6))); cd = cdi.Real + 0.00066f * Complex32.Pow((alpha - αd), (2.54f)); }
                if (Mr > 0.75f) { cdi = 0.0081f + (((-350 * alpha) + (396 * Complex32.Pow(alpha, 2)) - (63.3f * Complex32.Pow(alpha, 3)) + (3.66f * (Complex32.Pow(alpha, 4)))) * Mathf.Pow(10, (-6))); cd = cdi + 0.00035f * Complex32.Pow((alpha - αd), 2.54f) + 21 * Mathf.Pow((Mr - 0.725f), 3.2f); }
            }
            else { cd = 0.0081f + (((-350 * alpha) + (396 * Complex32.Pow(alpha, 2)) - (63.3f * Complex32.Pow(alpha, 3)) + (3.66f * (Complex32.Pow(alpha, 4)))) * Complex32.Pow(10, (-6))); }
            return cd.Real;
        }



        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static float Modulus(Complex32 value)
        {
            float m = Mathf.Sqrt((value.Real * value.Real) + (value.Imaginary * value.Imaginary));
            return m;
        }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static float Interpolate(float r1, float r2, float y1, float y2, float rx)
        {
            float a1 = (rx - r1) * (y2 - y1);
            float a2 = (r2 - r1);
            float yx = y1 + (a1 / a2);
            return yx;
        }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static float[] ArrayDot(float[] inputArray, float factor)
        {
            float[] values = new float[inputArray.Length];
            for (int i = 0; i < values.Length; i++) { values[i] = inputArray[i] * factor; }
            return values;
        }

        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static float[] LineSpace(float x1, float x2, int N, float factor)
        {
            float[] values = new float[N];
            float spacing = (x2 - x1) / (N - 1);
            for (int i = 0; i < N; i++) { values[i] = x1 + (i * spacing) - factor; }
            return values;
        }

        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static float[] Ones(int N)
        {
            float[] values = new float[N];
            for (int i = 0; i < N; i++) { values[i] = 1; }
            return values;
        }

        public static Complex32 ForwardInflow(Complex32 µ, Complex32 λh, Complex32 λc, Complex32 λ0)
        {
            // Control Factors
            bool finished = false; Complex32 λ = λ0;
            Complex32 αTPP = Complex32.Atan(λc / (µ + 0.00001f));
            int iterationCount = 0;
            // Iteration
            while (!finished)
            {
                Complex32 zero = λ - µ * Complex32.Tan(αTPP) - (λh * λh) / (Complex32.Sqrt((µ * µ) + (λ * λ)));
                if (zero.Real < 0.0001) { finished = true; } else { λ -= zero / 2; iterationCount++; }
                if (iterationCount >= maximumIterations) { finished = true; }
            }
            return λ;
        }


        public const int maximumIterations = 15;
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static Complex32 InflowSolver(Complex32 µ, Complex32 CT, Complex32 λc, Complex32 λ0)
        {
            // Control Factors
            bool finished = false; Complex32 λ = λ0;
            Complex32 α = Complex32.Atan(λc / (µ + 0.00001f));
            int iterationCount = 0;
            // Iteration
            while (!finished)
            {
                Complex32 zero = λ - µ * Complex32.Tan(α) - (CT) / (2 * Complex32.Sqrt((µ * µ) + (λ * λ)));
                if (zero.Real < 0.0001) { finished = true; } else { λ -= zero / 2; iterationCount++; }
                if (iterationCount >= maximumIterations) { finished = true; }
            }
            return λ;
        }




        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void PlotClimbCorrection(out AnimationCurve curve, float maximumClimbRate)
        {
            curve = new AnimationCurve();

            curve.AddKey(new Keyframe(-0.909f * maximumClimbRate, 1 / 0.649f));
            curve.AddKey(new Keyframe(-0.636f * maximumClimbRate, 1 / 0.707f));
            curve.AddKey(new Keyframe(-0.455f * maximumClimbRate, 1 / 0.773f));
            curve.AddKey(new Keyframe(-0.273f * maximumClimbRate, 1 / 0.847f));
            curve.AddKey(new Keyframe(0f, 1));
            curve.AddKey(new Keyframe(0.273f * maximumClimbRate, 1 / 1.751f));
            curve.AddKey(new Keyframe(0.455f * maximumClimbRate, 1 / 2.333f));
            curve.AddKey(new Keyframe(0.636f * maximumClimbRate, 1 / 3.492f));
            curve.AddKey(new Keyframe(0.818f * maximumClimbRate, 1 / 6.943f));
            curve.AddKey(new Keyframe(1.000f * maximumClimbRate, 0f));


#if UNITY_EDITOR
            for (int i = 0; i < curve.keys.Length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
            }
#endif
        }



        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void PlotInflowCorrection(out AnimationCurve curve)
        {
            curve = new AnimationCurve();

            curve.AddKey(new Keyframe(0f, 0.00632f));
            curve.AddKey(new Keyframe(5f, 0.00653f));
            curve.AddKey(new Keyframe(10f, 0.00703f));
            curve.AddKey(new Keyframe(15f, 0.00760f));
            curve.AddKey(new Keyframe(20f, 0.00813f));
            curve.AddKey(new Keyframe(25f, 0.00852f));
            curve.AddKey(new Keyframe(30f, 0.00880f));
            curve.AddKey(new Keyframe(35f, 0.00903f));
            curve.AddKey(new Keyframe(40f, 0.00912f));
            curve.AddKey(new Keyframe(45f, 0.00932f));
            curve.AddKey(new Keyframe(50f, 0.00942f));
            curve.AddKey(new Keyframe(55f, 0.00950f));
            curve.AddKey(new Keyframe(60f, 0.00958f));
            curve.AddKey(new Keyframe(65f, 0.00963f));
            curve.AddKey(new Keyframe(70f, 0.00969f));
            curve.AddKey(new Keyframe(75f, 0.00973f));
            curve.AddKey(new Keyframe(80f, 0.00977f));
            curve.AddKey(new Keyframe(85f, 0.00980f));
            curve.AddKey(new Keyframe(90f, 0.00983f));
            curve.AddKey(new Keyframe(95f, 0.00986f));
            curve.AddKey(new Keyframe(100f, 0.00988f));


#if UNITY_EDITOR
            for (int i = 0; i < curve.keys.Length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
            }
#endif
        }




        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void PlotGroundCorrection(out AnimationCurve curve)
        {
            curve = new AnimationCurve();

            curve.AddKey(new Keyframe(0.2402f, 1.3395f));
            curve.AddKey(new Keyframe(0.3672f, 1.2644f));
            curve.AddKey(new Keyframe(0.4908f, 1.2088f));
            curve.AddKey(new Keyframe(0.6174f, 1.1706f));
            curve.AddKey(new Keyframe(0.7343f, 1.1386f));
            curve.AddKey(new Keyframe(0.8576f, 1.1143f));
            curve.AddKey(new Keyframe(1.1657f, 1.0747f));
            curve.AddKey(new Keyframe(1.4769f, 1.047f));
            curve.AddKey(new Keyframe(1.7752f, 1.0282f));
            curve.AddKey(new Keyframe(2.0766f, 1.0144f));
            curve.AddKey(new Keyframe(2.6954f, 1.0097f));
            curve.AddKey(new Keyframe(3.0000f, 1.0000f));

#if UNITY_EDITOR
            for (int i = 0; i < curve.keys.Length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
            }
#endif
        }




        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void PlotCorrectionFactor(PhantomRotor.TandemAnalysisMethod method, out AnimationCurve correctionCurve)
        {
            correctionCurve = new AnimationCurve();


            // ------------------------- MT1
            if (method == PhantomRotor.TandemAnalysisMethod.MT1)
            {
                correctionCurve.AddKey(new Keyframe(0.0033567f, 1.4136316f));
                correctionCurve.AddKey(new Keyframe(0.0601530f, 1.3812821f));
                correctionCurve.AddKey(new Keyframe(0.1169700f, 1.3535766f));
                correctionCurve.AddKey(new Keyframe(0.1935351f, 1.3129231f));
                correctionCurve.AddKey(new Keyframe(0.2701039f, 1.2731984f));
                correctionCurve.AddKey(new Keyframe(0.3442051f, 1.2353244f));
                correctionCurve.AddKey(new Keyframe(0.4022476f, 1.2048359f));
                correctionCurve.AddKey(new Keyframe(0.4627769f, 1.1771408f));
                correctionCurve.AddKey(new Keyframe(0.5406064f, 1.1429924f));
                correctionCurve.AddKey(new Keyframe(0.6184319f, 1.1079151f));
                correctionCurve.AddKey(new Keyframe(0.6802066f, 1.0820810f));
                correctionCurve.AddKey(new Keyframe(0.7617795f, 1.0553733f));
                correctionCurve.AddKey(new Keyframe(0.8235695f, 1.0332545f));
                correctionCurve.AddKey(new Keyframe(0.9002226f, 1.0139634f));
                correctionCurve.AddKey(new Keyframe(0.9867998f, 1.0002728f));
                correctionCurve.AddKey(new Keyframe(1.0288680f, 0.9976036f));
            }
            // ------------------------- MT2
            if (method == PhantomRotor.TandemAnalysisMethod.MT2)
            {
                correctionCurve.AddKey(new Keyframe(0.0033412f, 1.4099164f));
                correctionCurve.AddKey(new Keyframe(0.0465359f, 1.3803154f));
                correctionCurve.AddKey(new Keyframe(0.0946735f, 1.3488706f));
                correctionCurve.AddKey(new Keyframe(0.1354006f, 1.3211203f));
                correctionCurve.AddKey(new Keyframe(0.1835459f, 1.2915331f));
                correctionCurve.AddKey(new Keyframe(0.2304727f, 1.2665864f));
                correctionCurve.AddKey(new Keyframe(0.2971518f, 1.2296205f));
                correctionCurve.AddKey(new Keyframe(0.3588881f, 1.1944984f));
                correctionCurve.AddKey(new Keyframe(0.4317747f, 1.1621937f));
                correctionCurve.AddKey(new Keyframe(0.4997107f, 1.1298753f));
                correctionCurve.AddKey(new Keyframe(0.5701564f, 1.1059230f));
                correctionCurve.AddKey(new Keyframe(0.6381269f, 1.0819638f));
                correctionCurve.AddKey(new Keyframe(0.7073350f, 1.0580080f));
                correctionCurve.AddKey(new Keyframe(0.7827504f, 1.0387135f));
                correctionCurve.AddKey(new Keyframe(0.8507630f, 1.0249711f));
                correctionCurve.AddKey(new Keyframe(0.9162965f, 1.0102930f));
                correctionCurve.AddKey(new Keyframe(0.9806116f, 1.0002555f));
                correctionCurve.AddKey(new Keyframe(1.0251550f, 0.9975933f));
            }
            // ------------------------- Harry
            if (method == PhantomRotor.TandemAnalysisMethod.Harris)
            {
                correctionCurve.AddKey(new Keyframe(0.0015595f, 1.2780229f));
                correctionCurve.AddKey(new Keyframe(0.0684034f, 1.2809955f));
                correctionCurve.AddKey(new Keyframe(0.1203842f, 1.2811403f));
                correctionCurve.AddKey(new Keyframe(0.1587510f, 1.2812472f));
                correctionCurve.AddKey(new Keyframe(0.1970488f, 1.2646357f));
                correctionCurve.AddKey(new Keyframe(0.2489338f, 1.2415604f));
                correctionCurve.AddKey(new Keyframe(0.2921477f, 1.2166035f));
                correctionCurve.AddKey(new Keyframe(0.4020637f, 1.1898165f));
                correctionCurve.AddKey(new Keyframe(0.4551748f, 1.1602534f));
                correctionCurve.AddKey(new Keyframe(0.5095083f, 1.1343952f));
                correctionCurve.AddKey(new Keyframe(0.5564619f, 1.1048252f));
                correctionCurve.AddKey(new Keyframe(0.5910391f, 1.0863802f));
                correctionCurve.AddKey(new Keyframe(0.6417133f, 1.0679007f));
                correctionCurve.AddKey(new Keyframe(0.7109176f, 1.0513236f));
                correctionCurve.AddKey(new Keyframe(0.7801372f, 1.0264390f));
                correctionCurve.AddKey(new Keyframe(0.8518896f, 1.0052697f));
                correctionCurve.AddKey(new Keyframe(0.9273816f, 0.9980393f));
                correctionCurve.AddKey(new Keyframe(1.0028699f, 0.9973208f));
            }

#if UNITY_EDITOR
            for (int i = 0; i < correctionCurve.keys.Length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(correctionCurve, i, AnimationUtility.TangentMode.Auto);
                AnimationUtility.SetKeyRightTangentMode(correctionCurve, i, AnimationUtility.TangentMode.Auto);
            }
#endif
        }









        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void DrawCorrectionCurves(out AnimationCurve swirlCorrection, out AnimationCurve powerCorrection, out AnimationCurve thrustCorrection)
        {
            swirlCorrection = new AnimationCurve();
            powerCorrection = new AnimationCurve();
            thrustCorrection = new AnimationCurve();

            //-----------------------------------SWIRL
            Keyframe a1 = new Keyframe(0.005f, 0.013f);
            Keyframe b1 = new Keyframe(0.010f, 0.025f);
            Keyframe c1 = new Keyframe(0.015f, 0.037f);
            Keyframe d1 = new Keyframe(0.020f, 0.049f);
            Keyframe e1 = new Keyframe(0.025f, 0.057f);
            Keyframe f1 = new Keyframe(0.030f, 0.063f);
            Keyframe g1 = new Keyframe(0.035f, 0.075f);
            Keyframe h1 = new Keyframe(0.040f, 0.086f);
            Keyframe i1 = new Keyframe(0.045f, 0.093f);
            Keyframe j1 = new Keyframe(0.050f, 0.11f);
            //PLOT
            swirlCorrection.AddKey(a1); swirlCorrection.AddKey(b1); swirlCorrection.AddKey(c1); swirlCorrection.AddKey(d1); swirlCorrection.AddKey(e1); swirlCorrection.AddKey(f1);
            swirlCorrection.AddKey(g1); swirlCorrection.AddKey(h1); swirlCorrection.AddKey(i1); swirlCorrection.AddKey(j1);

#if UNITY_EDITOR
            for (int i = 0; i < swirlCorrection.keys.Length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(swirlCorrection, i, AnimationUtility.TangentMode.Auto);
                AnimationUtility.SetKeyRightTangentMode(swirlCorrection, i, AnimationUtility.TangentMode.Auto);
            }
#endif


            //----------------------------------POWER
            Keyframe a2 = new Keyframe(0.3f, 1.010f);
            Keyframe b2 = new Keyframe(0.4f, 1.022f);
            Keyframe c2 = new Keyframe(0.5f, 1.040f);
            Keyframe d2 = new Keyframe(0.6f, 1.043f);
            Keyframe e2 = new Keyframe(0.7f, 1.058f);
            Keyframe f2 = new Keyframe(0.8f, 1.061f);
            Keyframe g2 = new Keyframe(0.9f, 1.064f);
            Keyframe h2 = new Keyframe(1.0f, 1.068f);
            Keyframe i2 = new Keyframe(1.1f, 1.073f);
            Keyframe j2 = new Keyframe(1.2f, 1.080f);
            //PLOT
            powerCorrection.AddKey(a2); powerCorrection.AddKey(b2); powerCorrection.AddKey(c2); powerCorrection.AddKey(d2); powerCorrection.AddKey(e2);
            powerCorrection.AddKey(f2); powerCorrection.AddKey(g2); powerCorrection.AddKey(h2); powerCorrection.AddKey(i2); powerCorrection.AddKey(j2);

#if UNITY_EDITOR
            for (int i = 0; i < powerCorrection.keys.Length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(powerCorrection, i, AnimationUtility.TangentMode.Auto);
                AnimationUtility.SetKeyRightTangentMode(powerCorrection, i, AnimationUtility.TangentMode.Auto);
            }
#endif




            //----------------------------------THRUST
            Keyframe j3 = new Keyframe(-0.06f, 1.340f);
            Keyframe k3 = new Keyframe(-0.05f, 1.286f);
            Keyframe l3 = new Keyframe(-0.04f, 1.232f);
            Keyframe m3 = new Keyframe(-0.03f, 1.178f);
            Keyframe n3 = new Keyframe(-0.02f, 1.121f);
            Keyframe o3 = new Keyframe(-0.01f, 1.067f);

            Keyframe a3 = new Keyframe(0.00f, 1.000f);

            Keyframe b3 = new Keyframe(0.01f, 0.9472f);
            Keyframe c3 = new Keyframe(0.02f, 0.8910f);
            Keyframe d3 = new Keyframe(0.03f, 0.7750f);
            Keyframe e3 = new Keyframe(0.04f, 0.7170f);
            Keyframe f3 = new Keyframe(0.05f, 0.6850f);
            Keyframe g3 = new Keyframe(0.06f, 0.6210f);
            Keyframe h3 = new Keyframe(0.07f, 0.5680f);
            Keyframe i3 = new Keyframe(0.08f, 0.5104f);

            thrustCorrection.AddKey(j3);
            thrustCorrection.AddKey(k3);
            thrustCorrection.AddKey(l3);
            thrustCorrection.AddKey(m3);
            thrustCorrection.AddKey(n3);
            thrustCorrection.AddKey(o3);
            thrustCorrection.AddKey(a3);
            thrustCorrection.AddKey(b3);
            thrustCorrection.AddKey(c3);
            thrustCorrection.AddKey(d3);
            thrustCorrection.AddKey(e3);
            thrustCorrection.AddKey(f3);
            thrustCorrection.AddKey(g3);
            thrustCorrection.AddKey(h3);
            thrustCorrection.AddKey(i3);

#if UNITY_EDITOR
            for (int i = 0; i < thrustCorrection.keys.Length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(thrustCorrection, i, AnimationUtility.TangentMode.Auto);
                AnimationUtility.SetKeyRightTangentMode(thrustCorrection, i, AnimationUtility.TangentMode.Auto);
            }
#endif
        }
    }



#if UNITY_EDITOR
    /// <summary>
    /// A custom property drawer for vectors type structures.
    /// </summary>
    /// <seealso cref="UnityEditor.PropertyDrawer" />
    [CustomPropertyDrawer(typeof(cfloat))]
    [CustomPropertyDrawer(typeof(Gain))]
    [CustomPropertyDrawer(typeof(GainVector))]
    public class VectorPropertyDrawer : PropertyDrawer
    {
        /// <summary>
        /// A dictionary lookup of field counts keyed by class type name.
        /// </summary>
        private static Dictionary<string, int> _fieldCounts = new Dictionary<string, int>();

        /// <summary>
        /// Called when the GUI is drawn.
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the property GUI.</param>
        /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
        /// <param name="label">The label of this property.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var fieldCount = GetFieldCount(property);

            Rect contentPosition = EditorGUI.PrefixLabel(position, label);
            float divider; if (contentPosition.width > 185) { divider = 2.5f; } else { divider = 2f; }

            EditorGUIUtility.labelWidth = 12f;
            float fieldWidth = contentPosition.width / divider;
            bool hideLabels = contentPosition.width < 185;
            contentPosition.width /= divider;

            using (var indent = new EditorGUI.IndentLevelScope(-EditorGUI.indentLevel))
            {
                for (int i = 0; i < fieldCount; i++)
                {
                    if (!property.NextVisible(true))
                    {
                        break;
                    }

                    label = EditorGUI.BeginProperty(contentPosition, new GUIContent(property.displayName), property);
                    EditorGUI.PropertyField(contentPosition, property, hideLabels ? GUIContent.none : label);
                    EditorGUI.EndProperty();

                    contentPosition.x += fieldWidth;
                }
            }
        }

        /// <summary>
        /// Gets the field count for the specified property.
        /// </summary>
        /// <param name="property">The property for which to get the field count.</param>
        /// <returns>The field count of the property.</returns>
        private static int GetFieldCount(SerializedProperty property)
        {
            int count;
            if (!_fieldCounts.TryGetValue(property.type, out count))
            {
                var children = property.Copy().GetEnumerator();
                while (children.MoveNext())
                {
                    count++;
                }

                _fieldCounts[property.type] = count;
            }

            return count;
        }
    }

#endif

}

